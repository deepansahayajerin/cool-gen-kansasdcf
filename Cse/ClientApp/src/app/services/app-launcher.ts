import { Injectable } from "@angular/core";
import { Launch, Dialog, RequestType } from "@adv-appmod/bphx-cool";

/**
 * Implements launch service
 */
@Injectable()
export class AppLauncher implements Launch
{
  launch(dialog: Dialog, command: { [name: string]: any })
  {
    if (command.transaction)
    {
      let program = command.transaction;

      if (command.arguments)
      {
        program += " " + command.arguments;
      }

      dialog.start(
      {
        action: RequestType.Start,
        commandLine: program,
        displayFirst: false,
        restart: false
      }).subscribe();
    }
    else if (command.url)
    {
      window.open(command.url, "_blank");
    }
    else if (command.program === "[Report]")
    {
      window.open("", "_blank").document.body.innerHTML =
        command.report.replace("[REPORT] ", "");
    }
    else if (command.program === "print")
    {
      var id = Number(command.id);
      var name = command.window;

      dialog.procedures.forEach(procedure =>
        procedure.windows?.forEach(window =>
          window.print = (procedure.id === id) && (window.name === name)));

      setTimeout(() => window.print());
    }
    else if (command.message)
    {
      alert(command.message);
    }
    else
    {
      alert("Unknown command: " + command.program);
    }
  }
}

