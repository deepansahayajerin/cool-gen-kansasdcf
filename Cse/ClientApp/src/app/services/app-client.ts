import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { HttpClientService } from "@adv-appmod/bphx-cool";
import { environment } from "src/environments/environment";
import { Observable } from "rxjs";

export interface AppInfo
{
  applicationName?: string;
  version?: string;
  environmentName?: string;
  connections?: string[];
  allowTrace?: boolean;
  allowChangeDate?: boolean;
  credits?: string;
  copyright?: string;
}

/**
 * Customizes client service.
 */
@Injectable()
export class AppClient extends HttpClientService
{
  constructor(http: HttpClient)
  {
    super(http);
    this.base = environment.api;
  }

  about(): Observable<AppInfo>
  {
    return this.http.get<AppInfo>(this.base + "about");
  }
}

