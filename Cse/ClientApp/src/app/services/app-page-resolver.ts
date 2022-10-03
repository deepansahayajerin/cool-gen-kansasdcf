import { environment } from "src/environments/environment";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { map, shareReplay } from "rxjs/operators";
import { NgModule, Injectable, Component, Inject } from "@angular/core";
import { PageComponent, PageResolver, Procedure, Window} from "@adv-appmod/bphx-cool";
import { VIEW_ACCESSOR, View, Page } from "@adv-appmod/bphx-cool";
import { PagesModule } from "../pages.module";
import { AppInfo } from "./app-client";

@Injectable()
export class AppPageResover implements PageResolver
{
  /**
   * An app info.
   */
  appInfo?: AppInfo;

  constructor(private http: HttpClient)
  {
  }

  /**
   * Resolves page component type by procedure and window.
   * @param dialect a dialect to use to resolve component.
   * @param procedure a `Procedure` instance to resolve component.
   * @param window a `Window` instance to resolve component.
   * @returns An `Observable` of `PageComponent`.
   */
  resolve(
    dialect: string,
    procedure: Procedure,
    window?: Window): Observable<PageComponent>
  {
    const online = procedure.type === "online";
    const name = online ?
      procedure.name + ".DEFAULT.component" :
      procedure.name + "/" + window.name + ".DEFAULT.component";

    return this.components[name] ??=
      this.resolveComponent(
        name,
        online ?
          procedure.name :
          procedure.name + "_" + window.name);
  }

  private components: { [name: string]: Observable<PageComponent> } = {};

  private resolveComponent(
    name: string,
    componentName: string): Observable<PageComponent>
  {
    const url = `${environment.pages}${name}.html?v=${this.appInfo?.version}`;

    return this.http.get(url, { responseType: "text" }).
      pipe(
        map(template =>
        {
          @Component(
          { 
            template: template, 
            preserveWhitespaces: true
          })
          class PageComponent extends Page
          {
            constructor(@Inject(VIEW_ACCESSOR) view: View)
            {
              super(view);
            }
          }

          Object.defineProperty(
            PageComponent,
            "name",
            { value: componentName });

          const components = [ PageComponent ];

          @NgModule(
          {
            imports: [ PagesModule ],
            declarations: components,
            exports: components,
            entryComponents: components
          })
          class PageModule
          {
          }

          const pageComponent =
          {
            component: PageComponent,
            //moduleRef: createNgModuleRef(PageModule)
          };

          return pageComponent;
        }),
        shareReplay(1));
  }
}
