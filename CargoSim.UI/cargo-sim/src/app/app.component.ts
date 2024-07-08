import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgxGraphModule } from '@swimlane/ngx-graph';
import { HttpService } from './core/services/http.service';
import { Endpoint } from './core/enums/endpoint';
import { Graph } from './models/graph';
import { links, nodes } from './data/graph-data';
import { CommonModule } from '@angular/common';
import { Subject, Subscription } from 'rxjs';
import * as shape from 'd3-shape';
import { HeaderComponent } from './layout/header/header.component';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { CargoRealtimeClientService } from './core/services/cargo-realtime-client.service';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [
    ToastModule,
    RouterOutlet,
    CommonModule,
    NgxGraphModule,
    HeaderComponent,
    ButtonModule,
  ],
  providers: [MessageService],
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'cargo-sim';
  messages: string[] = [];
  coins: string = '';
  nextMoveTimeSpan: string = '';

  center$: Subject<boolean> = new Subject();

  curve: any = shape.curveBundle;

  isGraphReady = false;

  graphNodes = nodes;
  graphLinks = links;

  httpService = inject(HttpService);
  messageService = inject(MessageService);
  cargoRealtimeClientService = inject(CargoRealtimeClientService);

  private messagesSubscription: Subscription = null!;
  private coinsSubscription: Subscription = null!;
  private nextMoveTimeSpanSubscription: Subscription = null!;

  private countdownInterval: any;

  ngOnInit(): void {
    this.subscribeToHubMessages();
  }

  ngOnDestroy(): void {
    this.messagesSubscription.unsubscribe();
    this.coinsSubscription.unsubscribe();
    this.nextMoveTimeSpanSubscription.unsubscribe();

    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }

  private subscribeToHubMessages(): void {
    this.messagesSubscription = this.cargoRealtimeClientService
      .getMessages()
      .subscribe({
        next: (message: string) => {
          this.messages.push(message);
        },
        error: (error: any) => {
          console.error('Error while looking for messages: ', error);
        },
      });

    this.coinsSubscription = this.cargoRealtimeClientService
      .getcoins()
      .subscribe({
        next: (coins: string) => {
          this.coins = coins;
        },
        error: (error: any) => {
          console.error('Error while looking for messages: ', error);
        },
      });

    this.nextMoveTimeSpanSubscription = this.cargoRealtimeClientService
      .getNextMoveTimeSpan()
      .subscribe({
        next: (nextMoveTimeSpan: string) => {
          this.setCountdown(nextMoveTimeSpan);
        },
        error: (error: any) => {
          console.error('Error while looking for messages: ', error);
        },
      });
  }

  centerGraph() {
    this.center$.next(true);
  }

  public initializeGrid(): void {
    this.httpService.get<Graph>(Endpoint.GRAPH).subscribe({
      next: (response) => {
        nodes.push.apply(nodes, response.nodes);

        links.push.apply(links, response.links);

        this.isGraphReady = true;
      },
    });
  }

  public startSimulation(): void {
    this.httpService.get<Graph>(Endpoint.SIM_START).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Simulation started!',
          detail: 'Simulation started.',
          life: 1500,
        });
      },
    });
  }

  public stopSimulation(): void {
    this.httpService.get<Graph>(Endpoint.SIM_STOP).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Simulation stopped!',
          detail: 'Simulation stopped.',
          life: 1500,
        });
      },
    });
  }

  public manuallyMove(): void {
    this.httpService.get<Graph>(Endpoint.MOVE).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Move called!',
          detail: 'Move called.',
          life: 1500,
        });
      },
      error: (error) => {
        this.messageService.add({
          severity: 'danger',
          summary: "Can't Move!",
          detail: "Can't Move.",
          life: 1500,
        });
      },
    });
  }

  public createOrder(): void {
    this.httpService.get(Endpoint.CREATE_ORDER).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Order created!',
          detail: 'Order created.',
          life: 500,
        });
      },
    });
  }

  private setCountdown(nextMoveTimeSpan: string): void {
    // Convert TimeSpan seconds
    const timeParts = nextMoveTimeSpan
      .split(':')
      .map((part) => parseInt(part, 10));
    let totalSeconds = timeParts[0] * 3600 + timeParts[1] * 60 + timeParts[2];

    // Clear any existing interval
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }

    // Update the display initially
    this.updateDisplay(totalSeconds);

    // Interval -1 every second
    this.countdownInterval = setInterval(() => {
      if (totalSeconds > 0) {
        totalSeconds--;
        this.updateDisplay(totalSeconds);
      } else {
        clearInterval(this.countdownInterval);
        // Wait for 2 seconds before calling manuallyMove, workaround to fix move endpoint when transporter InTransit
        setTimeout(() => {
          this.manuallyMove();
        }, 3000);
      }
    }, 1000);
  }

  private updateDisplay(totalSeconds: number): void {
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;
    this.nextMoveTimeSpan = `${this.pad(hours)}:${this.pad(minutes)}:${this.pad(
      seconds
    )}`;
  }

  private pad(num: number): string {
    return num < 10 ? '0' + num : num.toString();
  }
}
