import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { HubConnectionBuilder } from "@microsoft/signalr";

type counter = 
{
  key: string;
  counter: number;
  localcounter: number;
};

type postResponse =
{
  key: string;
  counter: number;
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
    this.http.get<string[]>("http://localhost:5000/realtime", {})
    .subscribe(response => 
      {
        response.forEach(key => {
          this.http.get<getResponse>("http://localhost:5000/realtime/" + key)
            .subscribe(response => 
            {
                this.counters.push({ key:key, counter:response.counter, localcounter:0 });
            });
        });
      });

    const connection = new HubConnectionBuilder()
    .withUrl("http://localhost:5000/counterhub")
    .build();

    connection.on("UpdateCounter", (key: string, count: number) =>
    {
      let row = this.counters.find(x => x.key == key);
      if (row != undefined)
      {
        row.counter = row.counter = count;
      }
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
      let row = { key: key, counter: 0, localcounter: 1 };
      this.counters.push(row);
      this.http.put<postResponse>("http://localhost:5000/realtime/" + key, {})
        .subscribe(response => 
          {
            row.counter = response.counter;
          });
    }

    this.key = "";
  }

  plus(key: string)
  {
    debugger;
    let row = this.counters.find(x => x.key == key);
    if (row != undefined)
    {
      //let innerRow = row;
      row.localcounter++;
      this.http.put<postResponse>("http://localhost:5000/realtime/" + key, {})
        .subscribe(response => 
          {
            //innerRow.counter = response.counter;
          });
    }
  }

  clear()
  {
    this.http.post("http://localhost:5000/realtime/clear", {})
      .subscribe(() => 
      {
        this.counters = [];
      });
  }
}