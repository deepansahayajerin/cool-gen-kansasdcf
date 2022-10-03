import { config, debug, showBoxTimeout } from "../../playwright.config";
import { ICustomWorld } from '../support/custom-world';
import { findField, findWindowByContent, keyInTextField, messageLocator, showBox, verifyFieldHasValue, verifyMessageIsDisplayed, waitForDialog, windowContentLocator } from "../lib";
import { unquote, getDateString } from "../lib/string-utils";
import { Given, Then, When } from '@cucumber/cucumber';
import { expect } from "@playwright/test";
import { join } from 'path';

Given(/^(?:Go to the ksdcf website)?(\s+with current date(?:\s+(.*))?)?$/, async function (this: ICustomWorld, date?: string)
{
  let url = config.use.baseURL;

  if (date)
  {
    const currentDate = getDateString(undefined, date, "yyyy-MM-dd");
    url = url + "&currentDate=" + currentDate;
  }
  console.log(`URL is ${ url }`);

  await this.page.goto(url);
});

Then(
  /^(?:take\s+|make\s+|get\s+|create\s+|new\s+)?(?:a\s+|the\s+)?snapshot(?:\s+(.*))?$/,
  async function (this: ICustomWorld, name?: string)
  {
    name = unquote(name);

    if (name)
    {
      await this.page.screenshot({ path: join('screenshots', `${ name }.png`) });
    }
    else
    {
      const image = await this.page.screenshot();
      image && (await this.attach(image, 'image/png'));
    }
  });

Then("debug", async function ()
{
  // eslint-disable-next-line no-debugger
  debugger;
});

Then(
  /^(.*)\s+window\s+appears?$/,
  async function (this: ICustomWorld, title: string)
  {
    await waitForDialog(this.page, this.network);

    const window = await findWindowByContent(this.page, title);

    this.currentWindow = window;
    this.currentField = null;
  });

When(
  /^(?:user\s+)?press(?:es)?\s+(.*)$/,
  async function (this: ICustomWorld, key: string)
  {
    await waitForDialog(this.page, this.network);
    await this.page.keyboard.press(unquote(key));
  });

When(
  /^(?:user\s+)?(?:enter|type)s?\s+(.*)\s+in(?:to)?\s+(.*)$/,
  async function (this: ICustomWorld, text: string, location: string)
  {
    await waitForDialog(this.page, this.network);

    const window = this.currentWindow;

    expect(window).not.toBeNull();

    const contentLocator = windowContentLocator(window);

    const field = await findField(
      contentLocator,
      unquote(location),
      {
        page: this.page,
        editable: true,
        current: this.currentField,
        debug,
        showBoxTimeout
      });

    this.currentField = field;
    await keyInTextField(field, unquote(text));
  });

When(
  /^(?:user\s+)?(?:(?:move|position|hover|step|go|goes?\s+to)s?\s+)(?:(?:cursor|pointer|arrow)\s+)?(.*)$/,
  async function (this: ICustomWorld, location: string)
  {
    await waitForDialog(this.page, this.network);

    const window = this.currentWindow;

    expect(window).not.toBeNull();

    const contentLocator = windowContentLocator(window);

    const field = await findField(
      contentLocator,
      unquote(location),
      {
        page: this.page,
        current: this.currentField,
        debug,
        showBoxTimeout
      });

    this.currentField = field;
  });

Then(
  /^(.*)\s+has\s+(?:a\s+|the\s+)?value\s+(.*)$/,
  async function (this: ICustomWorld, location: string, text: string)
  {
    await waitForDialog(this.page, this.network);

    const window = this.currentWindow;

    expect(window).not.toBeNull();

    const contentLocator = windowContentLocator(window);

    const field = await findField(
      contentLocator,
      unquote(location),
      {
        page: this.page,
        current: this.currentField,
        debug,
        showBoxTimeout
      });

    await verifyFieldHasValue(field, unquote(text));
  });

Then(
  /^message\s+(.*)\s+is\s+displayed$/,
  async function (this: ICustomWorld, message: string)
  {
    await waitForDialog(this.page, this.network);

    const window = this.currentWindow;

    expect(window).not.toBeNull();

    const locator = messageLocator(window);

    if (debug)
    {
      await showBox(this.page, locator, null, true);
    }

    await verifyMessageIsDisplayed(locator, unquote(message));
  });
