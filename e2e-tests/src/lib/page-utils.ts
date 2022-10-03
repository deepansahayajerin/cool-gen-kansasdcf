import { PageNetwork } from "./page-network";
import { getDateString, unquote } from "./string-utils";
import { Locator, Page } from "playwright";
import { expect } from "@playwright/test";

export async function waitForAngular(page: Page)
{
  await page.evaluate(async () =>
  {
    if ("getAllAngularTestabilities" in window)
    {
      await Promise.all(window["getAllAngularTestabilities"]().
        map(testability => new Promise((res) => testability.whenStable(res))));
    }
  });
}

export async function waitForDialog(page: Page, network?: PageNetwork): Promise<void>
{
  await waitForAngular(page);
  await network?.waitIdle();
  await page.locator(".coolDialog:not(.pending)").waitFor();

  if (await page.locator(".coolDialog.error").count())
  {
    const error = await page.locator("cool-error-box .details").textContent();

    throw new Error(error);
  }
}

export function windowLocator(
  pageOrLocator: Page | Locator,
  window: string,
  active = true): Locator
{
  return pageOrLocator.
    locator(`[coolwindow="${ window }"]${ active ? '.coolActive' : '' }`);
}

export async function findWindowByTitle(
  pageOrLocator: Page | Locator,
  title: string,
  active = true): Promise<Locator>
{
  const locator = pageOrLocator.
    locator(`[coolWindow]${ active ? '.coolActive' : ''
      }:has(header .title:has-text("${ unquote(title).replaceAll("\"", "\\\"")
      }"))`);

  const name = await locator.getAttribute("coolWindow");

  return windowLocator(pageOrLocator, name, active);
}

export async function findWindowByContent(
  pageOrLocator: Page | Locator,
  content: string,
  active = true): Promise<Locator>
{
  const locator = pageOrLocator.
    locator(`[coolWindow]${ active ? '.coolActive' : ''
      }:has(content:has-text("${ unquote(content).replaceAll("\"", "\\\"")
      }"))`);

  const name = await locator.getAttribute("coolWindow");

  return windowLocator(pageOrLocator, name, active);
}

export function windowContentLocator(locator: Locator): Locator
{
  return locator.locator("content");
}

export async function windowTitle(locator: Locator): Promise<string>
{
  const titleLocator = locator.locator("header .title");

  await titleLocator.hover();

  return await titleLocator.textContent();
}

export function fieldLocator(
  pageOrLocator: Page | Locator,
  field: string): Locator
{
  return pageOrLocator.locator(`[coolName="${ field }"]`);
}

export function positionalLocator(
  pageOrLocator: Page | Locator,
  row: string,
  col: string): Locator
{
  return pageOrLocator.locator(`[coolCol="${ col }"][coolRow="${ row }"]`);
}

export function messageLocator(pageOrLocator: Page | Locator): Locator
{
  return pageOrLocator.locator(`[coolmessagetype]`).first();
}

export async function keyInTextField(
  locator: Locator,
  text: string): Promise<void>
{
  await locator.hover();
  await locator.focus();
  await locator.fill("");

  if (((text.toUpperCase() === "TODAY") || 
    (text.toUpperCase() === "TOMORROW") || 
    (text.toUpperCase() === "YESTERDAY") || 
    (text.toUpperCase().includes("DAYS")) || 
    (text.toUpperCase().includes("MONTHS")) || 
    (text.toUpperCase().includes("YEARS")) || 
    (text.toUpperCase().includes("WEEKS"))) && 
    await locator.evaluateAll(([element]) => 
      element?.getAttribute("coolDate")))
  {
    text = getDateString(undefined, text);
  }

  await locator.type(text);
}

export async function verifyMessageIsDisplayed(
  locator: Locator,
  message: string): Promise<void>
{
  await locator.hover();
  await locator.locator(`text=${ message }`).waitFor();
}

export async function verifyFieldHasData(locator: Locator): Promise<void>
{
  await locator.hover();

  const text = await locator.inputValue({ timeout: 1000 });

  expect(text.length).toBeGreaterThan(0);
}

export async function verifyFieldHasValue(
  locator: Locator,
  text: string): Promise<void>
{
  await locator.hover();

  const actual = await locator.evaluate(
    element =>
    {
      switch(element?.nodeName?.toUpperCase())
      {
        case "INPUT":
        case "TEXTAREA":
        case "SELECT":
        {
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          return (element as any).value;
        }
        default:
        {
          return element?.textContent;
        }
      }
    },
    { timeout: 1000 });

  expect(actual).toEqual(text);
}

type Rect =
  {
    x: number;
    y: number;
    width: number;
    height: number;
  };

export async function showBox(
  page: Page,
  rectOrLocator: Rect | Locator,
  timeout?: number,
  removePrev?: boolean): Promise<void>
{
  if (!page)
  {
    return;
  }

  const rect = await (rectOrLocator as Locator)?.boundingBox?.() ??
    rectOrLocator as Rect;

  await page.evaluate(
    ({ rect, timeout, removePrev }) =>
    {
      if (removePrev)
      {
        document.querySelectorAll("playwright-debug-box").
          forEach(item => item.remove());
      }

      if (rect)
      {
        const box = document.createElement("playwright-debug-box");

        box.style.left = rect.x + "px";
        box.style.top = rect.y + "px";
        box.style.width = rect.width + "px";
        box.style.height = rect.height + "px";

        document.body.appendChild(box);

        if (timeout)
        {
          setTimeout(() => box.remove(), timeout);
        }
      }
    },
    { rect, timeout, removePrev });
}

// This injects a box into the page that moves with the mouse;
// Useful for debugging
export async function installHelpers(page: Page)
{
  await page.addInitScript(() =>
  {
    // Install mouse helper only for top-level frame.
    if (window !== window.parent)
      return;
    window.addEventListener('DOMContentLoaded', () =>
    {
      const box = document.createElement('playwright-mouse-pointer');
      const styleElement = document.createElement('style');
      styleElement.innerHTML = `
          playwright-mouse-pointer {
            pointer-events: none;
            position: absolute;
            top: 0;
            z-index: 10000;
            left: 0;
            width: 20px;
            height: 20px;
            background: rgba(0,0,0,.4);
            border: 1px solid white;
            border-radius: 10px;
            margin: -10px 0 0 -10px;
            padding: 0;
            transition: background .2s, border-radius .2s, border-color .2s;
          }
          playwright-mouse-pointer.button-1 {
            transition: none;
            background: rgba(0,0,0,0.9);
          }
          playwright-mouse-pointer.button-2 {
            transition: none;
            border-color: rgba(0,0,255,0.9);
          }
          playwright-mouse-pointer.button-3 {
            transition: none;
            border-radius: 4px;
          }
          playwright-mouse-pointer.button-4 {
            transition: none;
            border-color: rgba(255,0,0,0.9);
          }
          playwright-mouse-pointer.button-5 {
            transition: none;
            border-color: rgba(0,255,0,0.9);
          }

          playwright-debug-box {
            pointer-events: none;
            position: absolute;
            z-index: 10000;
            background: rgba(255,30,30,.25);
            border: 1px solid gold;
          }
          `;
      document.head.appendChild(styleElement);
      document.body.appendChild(box);
      document.addEventListener('mousemove', event =>
      {
        box.style.left = event.pageX + 'px';
        box.style.top = event.pageY + 'px';
        updateButtons(event.buttons);
      }, true);
      document.addEventListener('mousedown', event =>
      {
        updateButtons(event.buttons);
        box.classList.add('button-' + event.which);
      }, true);
      document.addEventListener('mouseup', event =>
      {
        updateButtons(event.buttons);
        box.classList.remove('button-' + event.which);
      }, true);
      function updateButtons(buttons)
      {
        for (let i = 0; i < 5; i++)
          box.classList.toggle('button-' + i, !!(buttons & (1 << i)));
      }
    }, false);
  });
}
