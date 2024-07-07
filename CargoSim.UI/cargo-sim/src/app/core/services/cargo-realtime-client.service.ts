import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CargoRealtimeClientService {
  private hubConnection: HubConnection = null!;

  private messagesSubject = new Subject<string>();
  private coinsSubject = new Subject<string>();
  private nextMoveTimeSpanSubject = new Subject<string>();

  constructor() {
    this.startConnection();
  }

  private async startConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('https://localhost:59067/message-hub')
      .build();

    try {
      await this.hubConnection.start();

      this.hubConnection.on('receive-console-message', (message: string) => {
        this.messagesSubject.next(message);
      });

      this.hubConnection.on('receive-coins', (coins: string) => {
        this.coinsSubject.next(coins);
      });

      this.hubConnection.on('receive-next-move-timespan', (nextMoveTimeSpan: string) => {
        this.nextMoveTimeSpanSubject.next(nextMoveTimeSpan);
      });

    } catch (error) {
      console.log('Error while establishing SignalR connection: ', error);
    }
  }

  public getMessages(): Observable<string> {
    return this.messagesSubject.asObservable();
  }

  public getcoins(): Observable<string> {
    return this.coinsSubject.asObservable();
  }

  public getNextMoveTimeSpan(): Observable<string> {
    return this.nextMoveTimeSpanSubject.asObservable();
  }
}
