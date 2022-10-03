import { PlaywrightTestConfig } from '@playwright/test';

export const debug = true;
export const showBoxTimeout = 500000;

const headless = false;
const timeout = 30000;
//const baseURL = "https://localhost:44303/#/start/CoCsmmChildSupMainMenu";
const baseURL = "https://ksdcf-dev.azurewebsites.net/#/start/CoCsmmChildSupMainMenu";

export const config: PlaywrightTestConfig =
{
  timeout,
  use:
  {
    baseURL,
    browserName: 'chromium',
    // viewport: 
    // {
    //   width: 1920,
    //   height: 1030
    // },
    launchOptions:
    {
      headless,
      timeout,
      slowMo: 0,
      args: ['--use-fake-ui-for-media-stream', '--use-fake-device-for-media-stream'],
      firefoxUserPrefs:
      {
        'media.navigator.streams.fake': true,
        'media.navigator.permission.disabled': true,
      },
    },

    contextOptions:
    {
      acceptDownloads: true,
      viewport: null,
      //recordVideo: process.env.PWVIDEO ? { dir: 'screenshots' } : undefined,
      recordVideo: { dir: 'screenshots' },
      httpCredentials:
      {
        username: 'SWCOJRE',
        password: '',
      },
    }
  },
};
