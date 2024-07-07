import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgxGraphModule, NgxGraphZoomOptions } from '@swimlane/ngx-graph';
import { HttpService } from './core/services/http.service';
import { Endpoint } from './core/enums/endpoint';
import { Graph } from './models/graph';
import { links, nodes } from './data/graph-data';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import * as shape from 'd3-shape';
import { HeaderComponent } from './layout/header/header.component';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

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
export class AppComponent {
  title = 'cargo-sim';

  center$: Subject<boolean> = new Subject();

  curve: any = shape.curveBundle;

  isGraphReady = false;

  graphNodes = nodes;
  graphLinks = links;

  httpService = inject(HttpService);
  messageService = inject(MessageService);

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
    });
  }
}
