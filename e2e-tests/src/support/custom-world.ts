import { PageNetwork } from "../lib";
import { Locator, BrowserContext, Page, Browser } from "playwright";
import { setWorldConstructor, World, IWorldOptions } from '@cucumber/cucumber';
import * as messages from '@cucumber/messages';

export interface CucumberWorldConstructorParams {
  parameters: { [key: string]: string };
}

export interface ICustomWorld extends World {
  debug: boolean;
  feature?: messages.Pickle;
  browser?: Browser;
  context?: BrowserContext;
  page?: Page;
  network?: PageNetwork;
  currentWindow?: Locator;
  currentField?: Locator;
  currentDate?: Date;
  testName?: string;
}

export class CustomWorld extends World implements ICustomWorld 
{
  constructor(options: IWorldOptions) 
  {
    super(options);
  }
  debug = false;
}

setWorldConstructor(CustomWorld);
