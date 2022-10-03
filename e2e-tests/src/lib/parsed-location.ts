import { Quadrant } from "./rect";
import { unquote } from "./string-utils";

enum TokenType
{
  Numeral,
  NumeralWord,
  NumeralDigits,
  FieldType,
  Label,
  LabelPrefixed,
  LabelQuoted,
  Left,
  Before,
  Right,
  Next,
  Above,
  Below,
  Near,
  Closest,
  Center,
  Window,
  Groupbox,
  Button,
  Link,
  Col,
  Row,
  Name,
  Current
}

const tokens = (() =>
{
  const parts: [RegExp, TokenType][] =
  [
    [/\b(first|second|third|forth)\b/, TokenType.NumeralWord],
    [/(\d+)-?(?:st|nd|rd|th)?/, TokenType.NumeralDigits],
    [/\b(text|field|control|textbox|input|edit|checkbox|radio|prompt|dropdown|combo|editable dropdown|editable combo|grid|table)s?\b/, TokenType.FieldType],
    [/\b(button)\b/, TokenType.Button],
    [/\b(anchor|link|menu)\b/, TokenType.Link],
    [/\blabel:?\s*(.*)\s*$/, TokenType.LabelPrefixed],
    [/('[^']+'|"[^"]+")/, TokenType.LabelQuoted],
    [/\b(left|prev)\b/, TokenType.Left],
    [/\b(before)\b/, TokenType.Before],
    [/\b(right|after)\b/, TokenType.Right],
    [/\b(next)\b/, TokenType.Next],
    [/\b(above|over)\b/, TokenType.Above],
    [/\b(below|under)\b/, TokenType.Below],
    [/\b(near|close)\b/, TokenType.Near],
    [/\b(nearest|closest)\b/, TokenType.Closest],
    [/\b(center|middle)\b/, TokenType.Center],
    [/\b(window|screen)\b/, TokenType.Window],
    [/\b(group\s*(?:box)?|fieldset)\b/, TokenType.Groupbox],
    [/\b(cols?|columns?|fields?)\b/, TokenType.Col],
    [/\b(rows?|lines?)\b/, TokenType.Row],
    [/\bname\s+(.*)\b/, TokenType.Name],
    [/\b(current|focus(?:ed)|cursor)\b/, TokenType.Current]
  ];

  const tokens =
  {
    pattern: new RegExp(parts.map(part => part[0].source).join("|"), "gi"),
    groups: parts.map(part => part[1]),
    selectors:
    {
      "text": "div[coolType]:not([coolContent]),span[coolType],label[coolType]:not(:has(input)), [coolType] legend",
      "field": "input[coolType][type=text],textarea[coolType]",
      "textbox": "input[coolType][type=text],textarea[coolType]",
      "input": "input[coolType][type=text],textarea[coolType]",
      "edit": "input[coolType][type=text],textarea[coolType]",
      "checkbox": "input[coolType][type=checkbox]",
      "radio": "input[coolType][type=radio]",
      "prompt": "[coolPrompt]",
      "dropdown": "select[coolType]",
      "combo": "select[coolType]",
      "editable dropdown": "input[coolType][type=text][list]",
      "editable combo": "input[coolType][type=text][list]",
      "grid": "[coolType=STNDLST]",
      "table": "[coolType=STNDLST]",
      "button": "button[coolType],input[type=button][coolType]",
      "anchor": "a[coolType]",
      "link": "a[coolType]",
      "menu": "a[coolType]",
    }
  };

  return tokens;
})();

export class ParsedLocation
{
  valid: boolean;
  error: string;
  current: boolean;
  text: string|string[];
  selector: string;
  col: number;
  row: number;
  quadrant: Quadrant;
  name: string;
  anchorText: string|string[];
  anchorQuadrant: Quadrant;

  constructor(public location: string)
  {
    let fieldType: string;
    let fieldNumber: number;
    let prevNumber: number;
    let prev2TokenType: TokenType;
    let prevTokenType: TokenType;
    let quadrant = Quadrant.None;
    let col: number;
    let row: number;
    let current: boolean;
    let text: string | string[];
    let anchorText: string|string[];
    let anchorQuadrant = Quadrant.None;
    let name: string;
    let locationAndField: boolean; // next field

    for(const match of location.matchAll(tokens.pattern))
    {
      const index = match.findIndex((value, index) => value && index);
      const value = match[index];
      let tokenType = tokens.groups[index - 1];

      switch(tokenType)
      {
        case TokenType.NumeralWord:
        {
          tokenType = TokenType.Numeral;

          const test = value.toLowerCase();

          prevNumber = test === "first" ? 1 : test === "second" ? 2 :
            test === "third" ? 3 : test === "forth" ? 4 : null;

          if (prevNumber == null)
          {
            this.error = `Invalid numeral ${ value }.`;

            return;
          }

          if (prev2TokenType != TokenType.Numeral)
          {
            if (prevTokenType == TokenType.Row)
            {
              row = prevNumber;
              prevTokenType = null;
            }
            else if (prevTokenType == TokenType.Col)
            {
              col = prevNumber;
              prevTokenType = null;
            }
          }

          break;
        }
        case TokenType.NumeralDigits:
        {
          tokenType = TokenType.Numeral;
          prevNumber = parseInt(value);

          if (prev2TokenType != TokenType.Numeral)
          {
            if (prevTokenType == TokenType.Row)
            {
              row = prevNumber;
              prevTokenType = null;
            }
            else if (prevTokenType == TokenType.Col)
            {
              col = prevNumber;
              prevTokenType = null;
            }
          }

          break;
        }
        case TokenType.FieldType:
        {
          if (current || fieldType)
          {
            continue;
          }

          if (prevTokenType === TokenType.Current)
          {
            current = true;
          }
          else
          {
            fieldType = value;

            if (prevTokenType === TokenType.Numeral)
            {
              fieldNumber = prevNumber;
            }
          }

          break;
        }
        case TokenType.LabelPrefixed:
        case TokenType.LabelQuoted:
        {
          tokenType = TokenType.Label;

          if (prevTokenType !== TokenType.Label)
          {
            if (text == null)
            {
              text = value;
            }
            else if (anchorText == null)
            {
              anchorText = value;
            }
            else
            {
              this.error = "Too many reference labels.";

              return;
            }
          }
          else
          {
            if (anchorText == null)
            {
              if (Array.isArray(text))
              {
                text.push(value);
              }
              else
              {
                text = [text, value];
              }
            }
            else
            {
              if (Array.isArray(anchorText))
              {
                anchorText.push(value);
              }
              else
              {
                anchorText = [anchorText, value];
              }
            }
          }

          break;
        }
        case TokenType.Col:
        {
          if (col != null)
          {
            this.error = "Multiple columns are specified.";

            return;
          }

          col = prevTokenType === TokenType.Numeral ? prevNumber : 1;

          break;
        }
        case TokenType.Row:
        {
          if (row != null)
          {
            this.error = "Multiple rows are specified.";

            return;
          }

          row = prevTokenType === TokenType.Numeral ? prevNumber : 1;

          break;
        }
        case TokenType.Left:
        {
          if (anchorText != null)
          {
            this.error = "Location after label reference.";

            return;
          }

          if (!fieldType)
          {
            locationAndField = true;
          }

          if (text == null)
          {
            quadrant |= Quadrant.West;
          }
          else
          {
            anchorQuadrant |= Quadrant.West;
          }

          break;
        }
        case TokenType.Before:
        {
          if (anchorText != null)
          {
            this.error = "Location after label reference.";

            return;
          }

          if (!fieldType)
          {
            locationAndField = true;
          }

          if (text == null)
          {
            quadrant |= Quadrant.Up | Quadrant.Middle | Quadrant.West;
          }
          else
          {
            anchorQuadrant = Quadrant.Closest | 
              Quadrant.Up | Quadrant.Middle | Quadrant.West;
          }

          break;
        }
        case TokenType.Right:
        {
          if (anchorText != null)
          {
            this.error = "Location after label reference.";

            return;
          }

          if (!fieldType)
          {
            locationAndField = true;
          }

          if (text == null)
          {
            quadrant |= Quadrant.East;
          }
          else
          {
            anchorQuadrant |= Quadrant.East;
          }

          break;
        }
        case TokenType.Next:
        {
          if (anchorText != null)
          {
            this.error = "Location after label reference.";

            return;
          }

          if (!fieldType)
          {
            locationAndField = true;
          }

          if (text == null)
          {
            quadrant |= Quadrant.East;
          }
          else
          {
            anchorQuadrant =  Quadrant.Closest | 
              Quadrant.Down | Quadrant.Middle | Quadrant.East;
          }

          break;
        }
        case TokenType.Above:
        {
          if (anchorText != null)
          {
            this.error = "Location after label reference.";

            return;
          }

          if (!fieldType)
          {
            locationAndField = true;
          }

          if (text == null)
          {
            quadrant |= Quadrant.North;
          }
          else
          {
            anchorQuadrant |= Quadrant.Closest | Quadrant.Up;
          }

          break;
        }
        case TokenType.Below:
        {
          if (anchorText != null)
          {
            this.error = "Location after label reference.";

            return;
          }

          if (!fieldType)
          {
            locationAndField = true;
          }

          if (text == null)
          {
            quadrant |= Quadrant.South;
          }
          else
          {
            anchorQuadrant |= Quadrant.Closest | Quadrant.Down;
          }

          break;
        }
        case TokenType.Near:
        {
          if (anchorText != null)
          {
            this.error = "Location after label reference.";

            return;
          }

          if (!fieldType)
          {
            locationAndField = true;
          }

          if (text == null)
          {
            quadrant = Quadrant.All;
          }
          else
          {
            anchorQuadrant = Quadrant.Closest | Quadrant.All;
          }

          break;
        }
        case TokenType.Closest:
        {
          if (anchorText != null)
          {
            this.error = "Location after label reference.";

            return;
          }

          if (!fieldType)
          {
            locationAndField = true;
          }

          if (text == null)
          {
            quadrant = Quadrant.Closest;
          }
          else
          {
            anchorQuadrant = Quadrant.Closest;
          }

          break;
        }
        case TokenType.Name:
        {
          name = match[11];

          break;
        }
        case TokenType.Current:
        {
          if (current != null)
          {
            this.error = "Multiple references to the current field.";

            return;
          }

          break;
        }
        default:
        {
          this.error = "Unrecognized term.";

          return;
        }
      }  

      prev2TokenType = prevTokenType;
      prevTokenType = tokenType;
    }

    if ((current == null) &&
      (anchorText == null) &&
      (((prevTokenType == TokenType.Current) && (text == null)) ||
        locationAndField))
    {
      current = true;
    }

    if (!fieldType)
    {
      fieldType = "field";

      if (current != null)
      {
        if (quadrant === Quadrant.None)
        {
          quadrant = Quadrant.East;
        }
      }
      else
      {
        quadrant = quadrant = Quadrant.All;
        text ??= location;
      }
    }

    const selector = tokens.selectors[fieldType];

    if (!selector)
    {
      this.error = "Invalid field type.";

      return;
    }

    if (!(quadrant & Quadrant.All))
    {
      quadrant |= Quadrant.All;
    }

    if ((quadrant & Quadrant.North) && (quadrant & Quadrant.South))
    {
      quadrant |= Quadrant.Middle;
    }

    if ((quadrant & Quadrant.West) && (quadrant & Quadrant.East))
    {
      quadrant |= Quadrant.Middle;
    }

    if ((quadrant & Quadrant.North) && (quadrant & Quadrant.West))
    {
      quadrant |= Quadrant.NorthWest;
    }

    if ((quadrant & Quadrant.North) && (quadrant & Quadrant.East))
    {
      quadrant |= Quadrant.NorthEast;
    }

    if ((quadrant & Quadrant.South) && (quadrant & Quadrant.West))
    {
      quadrant |= Quadrant.SouthWest;
    }

    if ((quadrant & Quadrant.South) && (quadrant & Quadrant.East))
    {
      quadrant |= Quadrant.SouthEast;
    }

    if (!(quadrant & Quadrant.Horizontal))
    {
      quadrant |= Quadrant.Middle;
    }
 
    if (((quadrant & Quadrant.All) === Quadrant.All) &&
      ((col != null) || (row != null) || (fieldNumber != null)))
    {
      this.error = "Near is not compatible with positional location.";

      return;
    }

    if (fieldNumber != null)
    {
      if ((col != null) && (row != null))
      {
        this.error = "Column, row and field order should not appear all together.";

        return;
      }

      if (quadrant & Quadrant.Vertical)
      {
        if (quadrant & Quadrant.Horizontal)
        {
          if (col != null)
          {
            this.error = "Column and field order are mutually exclusive";

            return;
          }

          col = fieldNumber;
        }
        else
        {
          if (row != null)
          {
            this.error = "Row and field order are mutually exclusive";

            row = fieldNumber;
          }
          else
          {
            row = fieldNumber;
          }
        }
      }
      else
      {
        if (col != null)
        {
          this.error = "Column and field order are mutually exclusive";

          return;
        }

        col = fieldNumber;
      }
    }

    if ((col != null) && (row != null) && !(quadrant & Quadrant.Vertical))
    {
      quadrant |= Quadrant.South;
    }

    text = Array.isArray(text) ? 
      text.map(item => unquote(item)) : unquote(text);
    anchorText = Array.isArray(anchorText) ? 
      anchorText.map(item => unquote(item)) : unquote(anchorText);

    if ((text == null) === !current)
    {
      if (current == null)
      {
        current = true;
      }

      else
      {
        this.error = "Neither current field nor label are specified";

        return;
      }
    }

    if ((anchorText != null) && current)
    {
      this.error = "Anchor text and current are mutually exclusive";

      return;
    }

    if (anchorText != null)
    {
      if (!(anchorQuadrant & Quadrant.All))
      {
        anchorQuadrant |= Quadrant.All;
      }

      if ((anchorQuadrant & Quadrant.North) && (anchorQuadrant & Quadrant.South))
      {
        anchorQuadrant |= Quadrant.Middle;
      }

      if ((anchorQuadrant & Quadrant.West) && (anchorQuadrant & Quadrant.East))
      {
        anchorQuadrant |= Quadrant.Middle;
      }

      if ((anchorQuadrant & Quadrant.North) && (anchorQuadrant & Quadrant.West))
      {
        anchorQuadrant |= Quadrant.NorthWest;
      }

      if ((anchorQuadrant & Quadrant.North) && (anchorQuadrant & Quadrant.East))
      {
        anchorQuadrant |= Quadrant.NorthEast;
      }

      if ((anchorQuadrant & Quadrant.South) && (anchorQuadrant & Quadrant.West))
      {
        anchorQuadrant |= Quadrant.SouthWest;
      }

      if ((anchorQuadrant & Quadrant.South) && (anchorQuadrant & Quadrant.East))
      {
        anchorQuadrant |= Quadrant.SouthEast;
      }

      if (!(anchorQuadrant & Quadrant.Horizontal))
      {
        anchorQuadrant |= Quadrant.Middle;
      }

      this.anchorQuadrant = anchorQuadrant;
    }

    this.selector = selector;
    this.name = name;
    this.current = current;
    this.text = text;
    this.anchorText = anchorText;
    this.quadrant = quadrant;
    this.row = row;
    this.col = col;
    this.valid = true;
  }
}
