import add from 'date-fns/add';
import { format } from 'date-fns';

export function unquote(value: string): string
{
  if (value)
  {
    value = value.trim();

    if (value.startsWith("\""))
    {
      if (value.indexOf("\"", 1) === value.length - 1)
      {
        value = value.substring(1, value.length - 1);
      }
    }
    else if (value.indexOf("'", 1) === value.length - 1)
    {
      if (value.endsWith("'"))
      {
        value = value.substring(1, value.length - 1);
      }
    }
  }

  return value;
}

export function getDateString(
  date?: Date,
  dateDescription?: string,
  dateFormat?: string): string
{
  date ??= new Date();
  dateFormat ??= "MMddyyyy";
  switch (unquote(dateDescription.toUpperCase()).trim())
  {
    case "TODAY":
      {
        return format(add(new Date(), { days: 0 }), dateFormat);
      }
    case "TOMORROW":
      {
        return format(add(new Date(), { days: 1 }), dateFormat);
      }
    case "YESTERDAY":
      {
        return format(add(new Date(), { days: -1 }), dateFormat);
      }
    default:
      {
        if (dateDescription.toUpperCase().includes("DAYS"))
        {
          //x Days after today or x Days later or x Days ago or x Days before today
          const numberOfDays = parseInt(dateDescription.replace(/\D/g, ''));

          if (dateDescription.toUpperCase().includes("AFTER"))
          {
            return format(add(new Date(), { days: numberOfDays }), dateFormat);
          }
          else
          {
            return format(add(new Date(), { days: -numberOfDays }), dateFormat);
          }
        }
        else if (dateDescription.toUpperCase().includes("MONTHS"))
        {
          //x months after today or x months later or x months ago or x months before today
          const numberOfMonths = parseInt(dateDescription.replace(/\D/g, ''));

          if (dateDescription.toUpperCase().includes("AFTER") ||
            dateDescription.toUpperCase().includes("LATER"))
          {
            return format(add(new Date(), { months: numberOfMonths }), dateFormat);
          }
          else
          {
            return format(add(new Date(), { months: -numberOfMonths }), dateFormat);
          }
        }
        else if (dateDescription.toUpperCase().includes("YEARS"))
        {
          //x years after today or x years later or x years ago or x years before today
          const numberOfYears = parseInt(dateDescription.replace(/\D/g, ''));

          if (dateDescription.toUpperCase().includes("AFTER") ||
            dateDescription.toUpperCase().includes("LATER"))
          {
            return format(add(new Date(), { years: numberOfYears }), dateFormat);
          }
          else
          {
            return format(add(new Date(), { years: -numberOfYears }), dateFormat);
          }
        }
        else if (dateDescription.toUpperCase().includes("WEEKS"))
        {
          //x weeks after today or x weeks later or x weeks ago or x weeks before today
          const numberOfWeeks = parseInt(dateDescription.replace(/\D/g, ''));

          if (dateDescription.toUpperCase().includes("AFTER") ||
            dateDescription.toUpperCase().includes("LATER"))
          {
            return format(add(new Date(), { weeks: numberOfWeeks }), dateFormat);
          }
          else
          {
            return format(add(new Date(), { weeks: -numberOfWeeks }), dateFormat);
          }
        }

        return dateDescription;
      }
  }
}
