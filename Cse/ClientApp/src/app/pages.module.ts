import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { ScrollingModule } from '@angular/cdk/scrolling';
import { BphxCoolModule, FlexGridModule } from "@adv-appmod/bphx-cool";

@NgModule(
{
  exports:
  [
    CommonModule,
    FormsModule,
    ScrollingModule,
    BphxCoolModule,
    FlexGridModule
  ]
})
export class PagesModule
{
}
