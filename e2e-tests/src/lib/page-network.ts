import { Page, Request } from "playwright";

export class PageNetwork
{
  constructor(public readonly page: Page)
  {
    page.on("request", this.start);
    page.on("requestfinished", this.end);
    page.on("requestfailed", this.end);
  }

  async close()
  {
    this.page.off("request", this.start);
    this.page.off("requestfinished", this.end);
    this.page.off("requestfailed", this.end);
  }

  async waitIdle(timeout?: number): Promise<void>
  {
    timeout ?
      await Promise.race([this.page.waitForTimeout(timeout), this.pending]) :
      await this.pending;
  }

  private start = (request: Request) => 
  {
    if (!this.requests.has(request))
    {
      this.requests.set(request, true);

      if (!this.requestsCount++)
      {
        this.pending = new Promise<void>(resolver => this.resolver = resolver);
      }
    }
  };
  
  private end = (request: Request) =>
  {
    if (this.requests.delete(request) && !--this.requestsCount)
    {
      const resolver = this.resolver;

      this.pending = null;
      this.resolver = null;
      resolver();
    }
  };

  private requestsCount = 0;
  private requests = new WeakMap<Request, boolean>();
  private pending: Promise<void>;
  private resolver: () => void;
}
