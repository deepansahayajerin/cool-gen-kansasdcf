import { ParsedLocation } from "./parsed-location";
import { fieldLocator, positionalLocator, showBox } from "./page-utils";
import { Rect, Quadrant, distanceMetric, compareMetrics, matchesMetric, ItemInfo } from "./rect";
import { Locator, Page } from "playwright";

export async function findField(
  locator: Locator,
  location: string,
  options?:
  {
    page?: Page,
    current?: Locator,
    required?: boolean,
    editable?: boolean,
    debug?: boolean,
    showBoxTimeout?: number
  }): Promise<Locator>
{
  function complete(result: Locator, message?: string): Locator
  {
    if (!result && (options?.required !== false))
    {
      throw Error(`No location "${ location }" is found over ${ 
        locator }.${ message ? "\n" + message : "" }`);
    }

    return result;
  }

  const parsedLocation = new ParsedLocation(location);

  if (!parsedLocation.valid)
  {
    return complete(null, parsedLocation.error);
  }

  if (parsedLocation.name)
  {
    return fieldLocator(locator, parsedLocation.name);
  }

  let anchorRect: Rect;
  let rect: Rect;
  
  try
  {
    anchorRect = parsedLocation.anchorText &&
      await getRect(locator, parsedLocation.anchorText);

    rect = anchorRect ||
      (await (parsedLocation.current ?
        getRect(options?.current) :
        getRect(locator, parsedLocation.text)) ??
        (parsedLocation.current &&
          await locator.evaluateAll(([container]) =>
          {
            const active = document.activeElement;

            if (active && container?.contains(active))
            {
              const { x, y, width, height } = active.getBoundingClientRect();

              return { x, y, width, height };
            }
          })));
  }
  catch(e)
  {
    return complete(null, e?.message);
  }

  if (parsedLocation.anchorText != null)
  {
    if (!anchorRect)
    {
      const text = typeof parsedLocation.anchorText === "string" ?
        parsedLocation.anchorText :
        parsedLocation.anchorText.join("', '");

      return complete(null, `No anchor '${text}' is found.`);
    }

    if (parsedLocation.text == null)
    {
      return complete(null, "No reference  is specified.");
    }

    const compareOptions =
    {
      quadrant: parsedLocation.anchorQuadrant,
      error: 1
    };

    rect = (await getRects(locator, parsedLocation.text)).
      filter(item => (item.width > 0) && (item.height > 0)).
      map(item => distanceMetric(rect, item, .05)).
      filter(item => matchesMetric(item, compareOptions.quadrant)).
      reduce(
        (c, i) => !c || (compareMetrics(c, i, compareOptions) > 0) ? i : c,
        null)?.item;
  }

  if (!rect)
  {
    if (parsedLocation.text != null)
    {
      const text = typeof parsedLocation.text === "string" ?
        parsedLocation.text :
        parsedLocation.text.join("', '");

      return complete(null, `No reference label '${text}' is found.`);
    }
    else
    {
      return complete(null, "No current field is found.");
    }
  }

  if (options?.page && options.debug)
  {
    await showBox(options.page, rect, options.showBoxTimeout, true);

    if (anchorRect)
    {
      await showBox(options.page, anchorRect, options.showBoxTimeout, false);
    }
  }

  const col = parsedLocation.col;
  let row = parsedLocation.row;
  let quadrant = parsedLocation.quadrant;

  if ((row != null) && (col != null))
  {
    if (quadrant & Quadrant.Down)
    {
      rect.y -= rect.height * row;
    }
    else
    {
      rect.y += rect.height * row;
    }

    row = null;
    quadrant = (quadrant & ~Quadrant.Vertical) | Quadrant.Middle;
  }

  const itemsLocator = locator.locator(parsedLocation.selector);
  
  const infos = await itemsLocator.evaluateAll(items => items.map(
    (element, index) =>
    {
      const { x, y, width, height } = element.getBoundingClientRect();
      const name = element.getAttribute("coolName");
      const col = element.getAttribute("coolCol");
      const row = element.getAttribute("coolRow");
      const tabindex = element.tabIndex;
      const readonly = (element as HTMLInputElement).readOnly;
      const disabled = (element as HTMLInputElement).disabled;
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const value = (element as any).value ?? element.textContent;

      const item: ItemInfo = 
      {
        index, 
        x,
        y, 
        width, 
        height, 
        name, 
        col, 
        row, 
        tabindex, 
        readonly, 
        disabled,
        value
      };

      return item;
    }));

  const matches = infos.
    filter(item =>
      (options.editable === true ? !item.disabled && !item.readonly :
      options.editable === false ? item.disabled || item.readonly :
      true) &&
      (item.width > 0) &&
      (item.height > 0) &&
      !((item.x === rect.x) && 
        (item.y === rect.y) && 
        (item.width === rect.width) && 
        (item.height === rect.height)) ).
    map(item => distanceMetric(rect, item, .05)).
    filter(item => matchesMetric(item, quadrant));

  const index = (col ?? row ?? 1) - 1;
  
  const compareOptions =
  {
    quadrant,
    error: 1,
    rect,
    anchorRect
  };

  if (index > 0)
  {
    matches.sort((first, second) => 
      compareMetrics(first, second, compareOptions));
  }

  const match = index > 0 ? matches[index]?.item :
    matches.reduce(
      (c, i) => !c || (compareMetrics(c, i, compareOptions) > 0) ? i : c,
      null)?.item;

  return !match ? complete(null, "No match is found.") :
    match.name ? fieldLocator(locator, match.name) :
    (match.col != null) && (match.row != null) ?
      positionalLocator(locator, match.row, match.col) :
      itemsLocator.nth(match.index);
}

async function getRect(locator: Locator, text?: string|string[]): Promise<Rect>
{
  if (!locator)
  {
    return null;
  }
  else if (!Array.isArray(text))
  {
    const rects = await (text ? locator.locator(`text=${ text }`) : locator).
      evaluateAll(items => items.map(item =>
      {
        const { x, y, width, height } = item.getBoundingClientRect();

        return { x, y, width, height, text: item.textContent };
      }));

    if (rects.length > 1)
    {
      const match = rects.filter(rect => rect.text === text);

      if (match.length === 1)
      {
        return match[0];
      }

      if (text)
      {
        throw new Error(`Multiple labels '${text}' are found.`);
      }
      else
      {
        throw new Error(`Multiple matches are found.`);
      }
    }

    return rects[0];
  }
  else
  {
    const rects = await Promise.all(text.map(item => 
      locator.locator(`text=${item}`).evaluateAll(items => items.map(item =>
      {
        const { x, y, width, height } = item.getBoundingClientRect();

        return { x, y, width, height };
      }))));

    let match: Rect;
    let matchSize: number;
    let nextMatch: Rect;
    let nextMatchSize: number;
    const indices = new Array<number>(rects.length).fill(0);

Search:    
    // eslint-disable-next-line no-constant-condition
    while(true)
    {
      const rect = indices.reduce((c, index, i) =>
      {
        const r = rects[i][index];

        if (!c)
        {
          return r;
        }

        const x = Math.min(c.x, r.x);
        const y = Math.min(c.y, r.y);
        const width = Math.max(c.x + c.width, r.x + r.width) - x;
        const height = Math.max(c.y + c.height, r.y + r.height) - y;

        return { x, y, width, height};
      }, null as Rect);

      const size = Math.hypot(rect.width, rect.height);

      if (!match || (matchSize > size))
      {
        nextMatch = match;
        nextMatchSize = matchSize;
        match = rect;
        matchSize = size;
      }

      for(let i = 0; i < indices.length; ++i)
      {
        if (++indices[i] < rects[i].length)
        {
          continue Search;
        }

        indices[i] = 0;
      }

      break;
    }

    if (match && nextMatch && (Math.abs(matchSize - nextMatchSize) < 1))
    {
      throw new Error(`Multiple labels '${text.join("', '")}' are found.`);
    }

    return match;
  }
}

async function getRects(locator: Locator, text?: string | string[]): Promise<Rect[]>
{
  if (!locator || !text)
  {
    return [];
  }
  else
  {
    const texts = Array.isArray(text) ? text : [text];

    const rects = await Promise.all(texts.map(item =>
      locator.locator(`text=${ item }`).evaluateAll(items => 
        items.map(item =>
        {
          const { x, y, width, height } = item.getBoundingClientRect();

          if ((width > 0) && (height > 0))
          {
            return { x, y, width, height };
          }
        }).
        filter(item => item))));

    const indices = new Array<number>(rects.length).fill(0);
    const matches: { rect: Rect, metric: number, indices: number[] }[] = [];

Search:
    // eslint-disable-next-line no-constant-condition
    while (true)
    {
      const rect = indices.reduce((c, index, i) =>
      {
        const r = rects[i][index];

        if (!c)
        {
          return r;
        }

        const x = Math.min(c.x, r.x);
        const y = Math.min(c.y, r.y);
        const width = Math.max(c.x + c.width, r.x + r.width) - x;
        const height = Math.max(c.y + c.height, r.y + r.height) - y;

        return { x, y, width, height };
      }, null as Rect);

      matches.push(
      {
        rect,
        metric: Math.hypot(rect.width, rect.height),
        indices: indices.slice()
      });

      for (let i = 0; i < indices.length; ++i)
      {
        if (++indices[i] < rects[i].length)
        {
          continue Search;
        }

        indices[i] = 0;
      }

      break;
    }

    return matches.
      sort((f, s) => f.metric - s.metric).
      filter(match =>
      {
        if (match.indices.some((index, i) => !rects[i][index]))
        {
          return false;
        }

        match.indices.forEach((index, i) => rects[i][index] = null);

        return true;
      }).
      map(match => match.rect);
  }
}
