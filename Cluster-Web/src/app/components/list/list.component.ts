import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { HubConnectionBuilder } from "@microsoft/signalr";
import { environment } from '../../../environments/environment';

type counter = 
{
  key: string;
  counter: number;
  localcounter: number;
  server: string;
  actorId?: number;
};

type incrementResponse = 
{
  Key: string;
  Counter: number;
  Server: string;
  ActorId?: number;
};

type getResponse = 
{
  key: string;
  counter: number;
};

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit {
  
  http: HttpClient;
  counters: counter[] = [];
  key: string = "";

  constructor(http: HttpClient) 
  {
    this.http = http;
  }

  ngOnInit(): void {
    this.http.get<string[]>(environment.api_url + "/realtime", {})
    .subscribe(response => 
      {
        response.forEach(key => {
          this.http.get<getResponse>(environment.api_url + "/realtime/" + key)
            .subscribe(response => 
            {
                this.counters.push({ key:key, counter:response.counter, localcounter:0, server: "" });
            });
        });
      });

    const connection = new HubConnectionBuilder()
      .withUrl(environment.api_url + "/counterhub")
      .build();

    connection.on("UpdateCounter", (key: string, count: number, incrementResponseText: string) =>
    {
      let incrementResponse: incrementResponse = JSON.parse(incrementResponseText);
      let row = this.counters.find(x => x.key == incrementResponse.Key);
      if (row != undefined)
      {
        row.counter = incrementResponse.Counter;
        row.server = incrementResponse.Server;
        row.actorId = incrementResponse.ActorId;
      }
      else
      {
        let row = { key: key, counter: incrementResponse.Counter, localcounter: 0, server: incrementResponse.Server, actorId: incrementResponse.ActorId };
        this.counters.push(row);
      }
    });

    connection.on("ClearCounters", () => 
    {
      this.counters = [];
    });
    
    connection.start().then(() => {})
    .catch(err => console.error(err));
  }

  add(key: string)
  {
    debugger; 
    let row = this.counters.find(x => x.key == key);
    if (row == undefined)
    {
      let row = { key: key, counter: 0, localcounter: 1, server: "" };
      this.counters.push(row);
      this.http.put(environment.api_url + "/realtime/" + key, {})
        .subscribe(response => 
        {});
    }

    this.key = "";
  }

  plus(key: string)
  {
    debugger;
    let row = this.counters.find(x => x.key == key);
    if (row != undefined)
    {
      row.localcounter++;
      this.http.put(environment.api_url + "/realtime/" + key, {})
        .subscribe(response => 
        {});
    }
  }

  clear()
  {
    this.http.post(environment.api_url + "/realtime/clear", {})
      .subscribe(() => 
      {});
  }
}