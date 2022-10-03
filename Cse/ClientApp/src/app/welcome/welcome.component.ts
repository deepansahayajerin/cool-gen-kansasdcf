import { HandleParams, RequestType } from '@adv-appmod/bphx-cool';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TouchSequence } from 'selenium-webdriver';
import { AppInfo } from '../services/app-client';
import { ProcedureInfo, startProcedures } from '../services/start-procedures';

@Component({
  selector: 'app-welcome',
  templateUrl: './welcome.component.html',
  styleUrls: ['./welcome.component.css']
})
export class WelcomeComponent implements OnInit {
  @Input() appInfo: AppInfo;
  @Output() start = new EventEmitter<HandleParams>();
  @Output() close = new EventEmitter();
  
  procedures: ProcedureInfo[] = startProcedures;
  startProcedure?: string;
  runAs?: string;
  currentDate?: string;
  commandLine?: string;
  newTrace: boolean = false;
  traceFile?: string;
  connection?: string;

  constructor() {}

  ngOnInit(): void {
  }

  ngAfterViewInit(): void
  {
  }

  startApplication()
  {
    let params: HandleParams = {
      action: RequestType.Start,
      procedureName: this.startProcedure
    }

    if (this.commandLine)
    {
      params.commandLine = this.commandLine;
    }

    if (this.runAs || this.traceFile || this.connection || this.currentDate) 
    {
      params.params = {};

      if (this.runAs)
      {
        params.params["userId"] = this.runAs;
      }

      if (this.traceFile)
      {
        params.params["traceId"] = this.traceFile;

        if (this.newTrace)
        {
          params.params["newTrace"] = "";
        }
      }

      if (this.connection)
      {
        params.params["mode"] = this.connection;
      }

      if (this.currentDate)
      {
        params.params["currentDate"] = this.currentDate;
      }
    }

    this.start.emit(params);
  }
}
