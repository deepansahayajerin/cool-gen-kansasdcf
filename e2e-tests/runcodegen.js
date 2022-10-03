import { config } from "./playwright.config";
import { chromium } from "playwright";

(async () => 
{
  // Make sure to run headed.
  const browser = await chromium.launch({ headless: false });

  // Setup context however you like.
  //const context = await browser.newContext({ /* pass any options */ });
  //await context.route('**/*', route => route.continue());
  const context = await browser.newContext({
    acceptDownloads: true,
    //recordVideo: process.env.PWVIDEO ? { dir: 'screenshots' } : undefined,
    httpCredentials: {
      username: "SWCOJRE",
      password: ""
    }
  });
  // Pause the page, and start recording manually.
  const page = await context.newPage();

  await page?.goto(config.use.baseURL);
  await page.pause();
})();