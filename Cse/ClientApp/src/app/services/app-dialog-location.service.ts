import { Location } from "@angular/common";
import { Injectable } from "@angular/core";
import { UrlDialogLocationService } from "@adv-appmod/bphx-cool";

/**
 * Dialog location based on browser's url location.
 */
@Injectable()
export class AppDialogLocationService extends UrlDialogLocationService
{
  autoStart: boolean = false;

  constructor(public location: Location)
  {
    super(location);
  }
}
