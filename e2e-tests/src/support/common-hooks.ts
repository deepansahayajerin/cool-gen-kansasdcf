import { ICustomWorld } from './custom-world';
import { PageNetwork, installHelpers, DbUtils } from "../lib";
import { config } from '../../playwright.config';
import { Before, After, BeforeAll, AfterAll, Status, setDefaultTimeout } from '@cucumber/cucumber';
import
{
  chromium,
  firefox,
  webkit,
  ConsoleMessage,
  Browser,
} from 'playwright';
import { ITestCaseHookParameter } from '@cucumber/cucumber/lib/support_code_library_builder/types';

import fs from 'fs-extra';

const tracesDir = 'traces';
let browser: Browser = null;

setDefaultTimeout(config.use.launchOptions.timeout ?? -1);

BeforeAll(async function ()
{
  console.log('****** in beforeALL');
  //new DbUtils().resetDB("KSDCFDB_E2E", "KSDCFDB_EBCIDIC");
  switch (config.use.browserName)
  {
    case 'firefox':
      browser = await firefox.launch(config.use.launchOptions);
      break;
    case 'webkit':
      browser = await webkit.launch(config.use.launchOptions);
      break;
    default:
      browser = await chromium.launch(config.use.launchOptions);
      break;
  }
  await fs.ensureDir(tracesDir);
});

Before({ tags: '@ignore' }, async function ()
{
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return 'skipped' as any;
});

Before({ tags: '@debug' }, async function (this: ICustomWorld)
{
  this.debug = true;
});

Before(async function (this: ICustomWorld, { pickle }: ITestCaseHookParameter)
{
  console.log('***********in Before');
  const time = new Date().toISOString().split('.')[0];
  this.testName = pickle.name.replace(/\W/g, '-') + '-' + time.replace(/:|T/g, '-');
  this.browser = browser;
  this.context = await this.browser.newContext(config.use.contextOptions);

  await this.context.tracing.start({ screenshots: true, snapshots: true });
  this.page = await this.context.newPage();
  this.page.setDefaultTimeout(config.timeout);
  this.network = new PageNetwork(this.page);
  installHelpers(this.page);

  this.page.on('console', async (msg: ConsoleMessage) =>
  {
    if (msg.type() === 'log')
    {
      await this.attach(msg.text());
    }
  });

  this.feature = pickle;
});

After(async function (this: ICustomWorld, { result }: ITestCaseHookParameter)
{
  if (result)
  {
    await this.attach(`Status: ${ result?.status }. Duration:${ result.duration?.seconds }s`);

    if (result.status !== Status.PASSED)
    {
      const image = await this.page?.screenshot();
      image && (await this.attach(image, 'image/png'));
      await this.context?.tracing.stop({ path: `${ tracesDir }/${ this.testName }-trace.zip` });
    }
  }

  await this.network?.close();
  await this.page?.close();
  await this.context?.close();
});

AfterAll(async function ()
{
  if (browser)
  {
    await browser.close();
    browser = null;
  }
});
