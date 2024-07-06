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

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, NgxGraphModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  title = 'cargo-sim';

  center$: Subject<boolean> = new Subject();

  curve: any = shape.curveBundle;

  isGraphReady = false;

  graphNodes = nodes;
  graphLinks = links;

  httpService = inject(HttpService);

  centerGraph() {
    this.center$.next(true);
  }

  public InitializeGrid(): void {
    this.httpService.get<Graph>(Endpoint.GRAPH).subscribe({
      next: (response) => {
        nodes.push.apply(nodes, response.nodes);

        links.push.apply(links, response.links);

        this.isGraphReady = true;
      },
    });
  }
}
