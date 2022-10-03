export interface DataItem
{
  col: number;
  row: number;
  type?: string;
  length?: number;
  value?: string;
  color?: string;
  editable?: boolean;
}

export async function collectData(
  options:
  {
    root?: HTMLElement|Document,
    readySelector?: string,
    errorSelector?: string,
    viewSelector: string,
    timeout?: number,
    testInterval?: number
  }): Promise<DataItem[][]>
{
  const root = options.root ?? document;
  const readySelector = options.readySelector ?? ".coolDialog.ready";
  const errorSelector = options.errorSelector ?? 
    ".coolDialog.error,.coolDialog.ended,.coolDialog.messageBox";
  const viewSelector = options.viewSelector;
  const timeout = options.timeout ?? 60000;
  const testInterval = options.testInterval ?? 200;
  const pages: DataItem[][] = [];
  let moreLabel: string = null;

  while(true)
  {
    const page = collectView();

    if (!page)
    {
      break;
    }

    pages.push(page);

    if (!hasMore())
    {
      break;
    }

    if (!await next())
    {
      break;
    }

    if (hasErrorMessage())
    {
      break;
    }
  }

  return pages;

  async function wait()
  {
    const start = Date.now();

    while(true)
    {
      await tick();

      if (Date.now() - start >= timeout)
      {
        throw new Error("Timeout while waiting for the data.");
      }

      if (isError())
      {
        throw new Error("Error occured while retrieving the data.");
      }

      if (isReady())
      {
        break;
      }
    }

    await tick();
  }

  async function tick()
  {
    await new Promise(resolve => window.setTimeout(resolve, testInterval));
  }

  function collectView(): DataItem[]
  {
    let pagination = false;

    moreLabel = null;

    const view = root.querySelector(viewSelector);

    if (!view)
    {
      return null;
    }

    const items: DataItem[] = [];

    view.
      querySelectorAll("[coolType]").
      forEach(element => 
      {
        const item: DataItem =
        {
          col: Number(element.getAttribute("coolCol")),
          row: Number(element.getAttribute("coolRow")),
          length: Number(element.getAttribute("coolLength")),
          type: element.getAttribute("coolType")
        }

        switch(element.nodeName?.toUpperCase())
        {
          case "INPUT":
          case "TEXTAREA":
          case "SELECT":
          {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            const input = element as any; 
            const value: string = input.value;

            item.value = value;

            if (!input.readOnly && !input.disabled)
            {
              item.editable = true;
            }

            break;
          }
          default:
          {
            item.value = element.textContent;

            break;
          }
        }
  
        const color = element.getAttribute("coolColor");

        if (color)
        {
          item.color = color;
        }

        if (item.value)
        {
          if (!pagination)
          {
            if (item.type === "pagination")
            {
              moreLabel = item.value;
              pagination = true;            
            }
            else if (item.value.match(/^\s*MORE[: \\/+-]+\s*$/i))
            {
              moreLabel = item.value;
            }
            // No more cases
          }

          items.push(item);
        }
      });

    return items;
  }

  function hasMore(): boolean
  {
    return !!moreLabel?.match(/\+/);
  }

  function hasErrorMessage(): boolean
  {
    return !!root.querySelector(viewSelector)?.
      querySelector("[coolMessageType=error]");
  }

  async function next(): Promise<boolean>
  {
    if (!root.querySelector(viewSelector).dispatchEvent(
      new KeyboardEvent("keydown", { key: "F8", bubbles: true })))
    {
      return false;
    }

    await wait();

    return true;
  }

  function isReady(): boolean
  {
    return !!root.querySelector(readySelector);
  }

  function isError(): boolean
  {
    return !!root.querySelector(errorSelector);
  }
}
