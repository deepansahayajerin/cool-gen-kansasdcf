import { PagesModule } from "./pages.module";
import { NgModule } from "@angular/core";
import { LocationStrategy, HashLocationStrategy } from "@angular/common";
import { Location } from "@angular/common";
import { BrowserModule } from "@angular/platform-browser";
import { FormsModule } from "@angular/forms";
import { HttpClientModule } from "@angular/common/http";
import { BphxCoolModule, DIALOG_LOCATION_ACCESSOR } from "@adv-appmod/bphx-cool";
import { PAGE_RESOLVER, CLIENT_ACCESSOR, LAUNCH_ACCESSOR } from "@adv-appmod/bphx-cool";
import { AppClient } from "./services/app-client";
import { AppComponent } from "./app.component";
import { AppPageResover } from "./services/app-page-resolver";
import { AppLauncher } from "./services/app-launcher";
import { AppDialogLocationService } from "./services/app-dialog-location.service";
import { WelcomeComponent } from './welcome/welcome.component';

@NgModule(
{
  declarations: [AppComponent, WelcomeComponent],
  imports:
  [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    BphxCoolModule,
    PagesModule
  ],
  providers:
  [
    Location,
    { provide: LocationStrategy, useClass: HashLocationStrategy },
    { provide: PAGE_RESOLVER, useClass: AppPageResover },
    { provide: CLIENT_ACCESSOR, useClass: AppClient },
    { provide: LAUNCH_ACCESSOR, useClass: AppLauncher },
    { provide: DIALOG_LOCATION_ACCESSOR, useClass: AppDialogLocationService },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
