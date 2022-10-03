import { AfterViewInit, Component, Inject, OnInit, Optional, ViewChild, ViewEncapsulation } from "@angular/core";
import { CLIENT_ACCESSOR, DialogComponent, PAGE_RESOLVER, SESSION_ACCESSOR, StateService, Window } from "@adv-appmod/bphx-cool";
import { collectData } from "./services/data-collector";
import { AppClient, AppInfo } from "./services/app-client";
import { AppPageResover } from "./services/app-page-resolver";
@Component(
{
  selector: "app-root,[app-root]",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css"],
  encapsulation: ViewEncapsulation.None
})
export class AppComponent implements OnInit, AfterViewInit
{
  appInfo: AppInfo;

  @ViewChild(DialogComponent, { static: true })
  dialog: DialogComponent;

  constructor(
    @Optional() @Inject(SESSION_ACCESSOR) public sessionState: StateService,
    @Inject(CLIENT_ACCESSOR) public client: AppClient,
    @Inject(PAGE_RESOLVER) public pageResolver: AppPageResover)
  {
    this.showWelcome = false;
  }

  get showWelcome(): boolean
  {
    let value = this.sessionState.get("app:showWelcome", true);
    
    return this.allowWelcomeScreen ? value : false;
  }

  set showWelcome(value: boolean)
  {
    this.sessionState.set("app:showWelcome", value);
  }

  get allowWelcomeScreen(): boolean
  {
    return (this.appInfo?.environmentName && 
      !this.appInfo?.environmentName?.startsWith("prod"));
  }

  ngOnInit(): void
  {
    this.client.about().
      subscribe(appInfo => this.appInfo = this.pageResolver.appInfo = appInfo);
  }

  ngAfterViewInit(): void
  {
    if (this?.dialog?.initAction.action == null)
    {
      this.showWelcome = true;
    }
  }

  windowId(index: number, window: Window)
  {
    return window.id;
  }

  restart()
  {
    if (this.allowWelcomeScreen)
    {
      this.showWelcome = true;
      this.dialog.windows = [];
      this.dialog.procedures = [];
    }
    else
    {
      this.dialog.start().subscribe();
    }
  }

  async print(window: Window): Promise<void>
  {
    try
    {
      const result = await collectData(
      {
        viewSelector: `[coolProcedureId="${window.procedure.id}"] [coolContent]`
      });

      console.log("Print data:");
      console.log(result);
    }
    catch(e)
    {
      console.log("Error has happened during collect of print data:");
      console.log(e);
    }

    await new Promise(resolve => setTimeout(resolve, 100));

    alert("Please open development console Ctrl+Shift+I to see collected print data.");
  }
}
