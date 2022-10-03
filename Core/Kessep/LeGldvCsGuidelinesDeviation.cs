// Program: LE_GLDV_CS_GUIDELINES_DEVIATION, ID: 1625350692, model: 746.
// Short name: SWEGLDVP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_GLDV_CS_GUIDELINES_DEVIATION.
/// </para>
/// <para>
/// Resp:OBLGEST
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeGldvCsGuidelinesDeviation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_GLDV_CS_GUIDELINES_DEVIATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeGldvCsGuidelinesDeviation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeGldvCsGuidelinesDeviation.
  /// </summary>
  public LeGldvCsGuidelinesDeviation(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date	    Developer		       Description
    // 07/10/2019  Dwayne Dupree		Initial Code
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.CswlCheck.Text1 = import.CswlCheck.Text1;
    export.FromLact.Text1 = import.FromLact.Text1;
    export.FromLacs.Text1 = import.FromLacs.Text1;
    export.PageCount.Count = import.PageCount.Count;
    export.LactCase.Number = import.LactCase.Number;
    export.LactCaseRole.Type1 = import.LactCaseRole.Type1;
    export.LactCsePerson.Number = import.LactCsePerson.Number;
    export.CswlReturn.Timestamp = import.CswlReturn.Timestamp;
    export.GuidelineDeviations.Assign(import.GuidelineDeviations);
    export.IvdAttorney.Assign(import.IvdAttorney);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    local.Compare.Timestamp = Now().AddHours(-1);
    export.Group.Index = -1;
    export.Group.Count = 0;

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      export.Group.Index = import.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.Answer.Text1 = import.Group.Item.Answer.Text1;
      export.Group.Update.Att.SystemGeneratedId =
        import.Group.Item.Att.SystemGeneratedId;
      export.Group.Update.Question.Text50 = import.Group.Item.Question.Text50;
      export.Group.Update.Legend.Text3 = import.Group.Item.Legend.Text3;
    }

    import.Group.CheckIndex();

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      export.Hidden.CaseNumber = export.LactCase.Number;
      export.Hidden.CsePersonNumber = export.LactCsePerson.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (!IsEmpty(export.LactCsePerson.Number))
    {
      local.CsePersonsWorkSet.Number = export.LactCsePerson.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "LIST"))
    {
      local.Row.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < Export
        .GroupGroup.Capacity; ++export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        ++local.Row.Count;

        switch(local.Row.Count)
        {
          case 1:
            break;
          case 2:
            break;
          case 3:
            break;
          case 4:
            break;
          case 5:
            switch(AsChar(export.Group.Item.Answer.Text1))
            {
              case ' ':
                break;
              case '+':
                break;
              case 'S':
                ExitState = "ECO_LNK_TO_SVPL";

                return;
              default:
                var field = GetField(export.Group.Item.Answer, "text1");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

                return;
            }

            break;
          case 6:
            break;
          case 7:
            break;
          case 8:
            break;
          case 9:
            break;
          case 10:
            break;
          case 11:
            break;
          case 12:
            break;
          case 13:
            break;
          default:
            break;
        }
      }

      export.Group.CheckIndex();
      ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

      return;
    }

    if (Equal(global.Command, "RETSVPL"))
    {
      local.Row.Count = 0;

      for(export.Group.Index = 0; export.Group.Index < Export
        .GroupGroup.Capacity; ++export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        ++local.Row.Count;

        switch(local.Row.Count)
        {
          case 1:
            var field1 = GetField(export.Group.Item.Answer, "text1");

            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;

            break;
          case 2:
            var field2 = GetField(export.Group.Item.Answer, "text1");

            field2.Highlighting = Highlighting.Underscore;
            field2.Protected = false;

            break;
          case 3:
            var field3 = GetField(export.Group.Item.Answer, "text1");

            field3.Highlighting = Highlighting.Underscore;
            field3.Protected = false;

            break;
          case 4:
            var field4 = GetField(export.Group.Item.Answer, "text1");

            field4.Highlighting = Highlighting.Underscore;
            field4.Protected = false;

            break;
          case 5:
            var field5 = GetField(export.Group.Item.Answer, "text1");

            field5.Highlighting = Highlighting.Underscore;
            field5.Protected = false;

            export.Group.Update.Answer.Text1 = "+";

            break;
          case 6:
            var field6 = GetField(export.Group.Item.Question, "text50");

            field6.Color = "green";
            field6.Protected = true;

            export.Group.Update.Question.Text50 =
              TrimEnd(import.IvdAttorney.FirstName) + " " + import
              .IvdAttorney.MiddleInitial + " " + TrimEnd
              (import.IvdAttorney.LastName);
            export.Group.Update.Att.SystemGeneratedId =
              import.IvdAttorney.SystemGeneratedId;
            export.GuidelineDeviations.IvDAttorney =
              import.IvdAttorney.SystemGeneratedId;
            export.Group.Update.Answer.Text1 = "";

            var field7 = GetField(export.Group.Item.Answer, "text1");

            field7.Highlighting = Highlighting.Normal;
            field7.Protected = true;

            break;
          case 7:
            var field8 = GetField(export.Group.Item.Answer, "text1");

            field8.Highlighting = Highlighting.Underscore;
            field8.Protected = false;

            break;
          case 8:
            var field9 = GetField(export.Group.Item.Answer, "text1");

            field9.Highlighting = Highlighting.Underscore;
            field9.Protected = false;

            break;
          case 9:
            var field10 = GetField(export.Group.Item.Answer, "text1");

            field10.Highlighting = Highlighting.Underscore;
            field10.Protected = false;

            break;
          case 10:
            var field11 = GetField(export.Group.Item.Answer, "text1");

            field11.Protected = true;

            break;
          case 11:
            var field12 = GetField(export.Group.Item.Answer, "text1");

            field12.Highlighting = Highlighting.Underscore;
            field12.Protected = false;

            break;
          case 12:
            var field13 = GetField(export.Group.Item.Answer, "text1");

            field13.Highlighting = Highlighting.Normal;
            field13.Protected = true;

            break;
          case 13:
            var field14 = GetField(export.Group.Item.Answer, "text1");

            field14.Highlighting = Highlighting.Underscore;
            field14.Protected = false;

            break;
          default:
            break;
        }
      }

      export.Group.CheckIndex();
      global.Command = "";
    }

    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "FIRSTIME":
        export.PageCount.Count = 1;
        local.Row.Count = 0;
        export.Group.Index = -1;
        export.Group.Count = 0;

        for(export.Group.Index = 0; export.Group.Index < Export
          .GroupGroup.Capacity; ++export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          ++local.Row.Count;

          switch(local.Row.Count)
          {
            case 1:
              export.Group.Update.Question.Text50 =
                "Did NCP attend the hearing?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field1 = GetField(export.Group.Item.Answer, "text1");

              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;

              break;
            case 2:
              export.Group.Update.Question.Text50 =
                "Did CP attend the hearing?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field2 = GetField(export.Group.Item.Answer, "text1");

              field2.Highlighting = Highlighting.Underscore;
              field2.Protected = false;

              break;
            case 3:
              export.Group.Update.Question.Text50 =
                "Did NCP attorney attend the hearing?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field3 = GetField(export.Group.Item.Answer, "text1");

              field3.Highlighting = Highlighting.Underscore;
              field3.Protected = false;

              break;
            case 4:
              export.Group.Update.Question.Text50 =
                "Did CP attorney attend the hearing?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field4 = GetField(export.Group.Item.Answer, "text1");

              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;

              break;
            case 5:
              export.Group.Update.Question.Text50 =
                "Which IV-D attorney attended the hearing?";
              export.Group.Update.Legend.Text3 = "";
              export.Group.Update.Answer.Text1 = "+";

              var field5 = GetField(export.Group.Item.Answer, "text1");

              field5.Highlighting = Highlighting.Underscore;
              field5.Protected = false;

              break;
            case 6:
              export.Group.Update.Question.Text50 = "";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              var field6 = GetField(export.Group.Item.Answer, "text1");

              field6.Protected = true;

              break;
            case 7:
              export.Group.Update.Question.Text50 = "Is the NCP incarcerated?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field7 = GetField(export.Group.Item.Answer, "text1");

              field7.Highlighting = Highlighting.Underscore;
              field7.Protected = false;

              break;
            case 8:
              export.Group.Update.Question.Text50 =
                "Was the income imputed for NCP?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field8 = GetField(export.Group.Item.Answer, "text1");

              field8.Highlighting = Highlighting.Underscore;
              field8.Protected = false;

              break;
            case 9:
              export.Group.Update.Question.Text50 =
                "Was the income imputed for CP?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field9 = GetField(export.Group.Item.Answer, "text1");

              field9.Highlighting = Highlighting.Underscore;
              field9.Protected = false;

              break;
            case 10:
              export.Group.Update.Question.Text50 =
                "Is the CS worksheet the same";

              var field10 = GetField(export.Group.Item.Answer, "text1");

              field10.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            case 11:
              export.Group.Update.Question.Text50 =
                "   as filed with petition?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field11 = GetField(export.Group.Item.Answer, "text1");

              field11.Highlighting = Highlighting.Underscore;
              field11.Protected = false;

              break;
            case 12:
              export.Group.Update.Question.Text50 = "Did the CS worksheet have";

              var field12 = GetField(export.Group.Item.Answer, "text1");

              field12.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            case 13:
              export.Group.Update.Question.Text50 = "   any adjustments?";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "Y/N";

              var field13 = GetField(export.Group.Item.Answer, "text1");

              field13.Highlighting = Highlighting.Underscore;
              field13.Protected = false;

              break;
            default:
              export.Group.Update.Question.Text50 = "";
              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              var field14 = GetField(export.Group.Item.Answer, "text1");

              field14.Protected = true;

              break;
          }
        }

        export.Group.CheckIndex();

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        if (ReadLegalAction())
        {
          MoveLegalAction(entities.LegalAction, export.LegalAction);

          if (ReadGuidelineDeviations())
          {
            export.GuidelineDeviations.Assign(entities.GuidelineDeviations);
            local.CsePersonsWorkSet.Number =
              entities.GuidelineDeviations.CsePersonNumber ?? Spaces(10);
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.LactCase.Number =
              entities.GuidelineDeviations.CseCaseNumber ?? Spaces(10);
          }
          else
          {
            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;

            ExitState = "GUIDELINE_DEVIATION_NF";

            return;
          }

          local.Row.Count = 0;

          for(export.Group.Index = 0; export.Group.Index < Export
            .GroupGroup.Capacity; ++export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            ++local.Row.Count;

            switch(local.Row.Count)
            {
              case 1:
                export.Group.Update.Question.Text50 =
                  "Did NCP attend the hearing?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.NcpHearing;
                export.Group.Update.Legend.Text3 = "Y/N";

                var field1 = GetField(export.Group.Item.Answer, "text1");

                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = true;

                break;
              case 2:
                export.Group.Update.Question.Text50 =
                  "Did CP attend the hearing?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.CpHearing ?? Spaces(1);
                export.Group.Update.Legend.Text3 = "Y/N";

                var field2 = GetField(export.Group.Item.Answer, "text1");

                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = true;

                break;
              case 3:
                export.Group.Update.Question.Text50 =
                  "Did NCP attorney attend the hearing?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.NcpAttorney ?? Spaces(1);
                export.Group.Update.Legend.Text3 = "Y/N";

                var field3 = GetField(export.Group.Item.Answer, "text1");

                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = true;

                break;
              case 4:
                export.Group.Update.Question.Text50 =
                  "Did CP attorney attend the hearing?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.CpAttorney ?? Spaces(1);
                export.Group.Update.Legend.Text3 = "Y/N";

                var field4 = GetField(export.Group.Item.Answer, "text1");

                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = true;

                break;
              case 5:
                export.Group.Update.Question.Text50 =
                  "Which IV-D attorney attended the hearing?";

                var field5 = GetField(export.Group.Item.Answer, "text1");

                field5.Protected = true;

                export.Group.Update.Legend.Text3 = "";

                break;
              case 6:
                if (ReadServiceProvider1())
                {
                  export.Group.Update.Question.Text50 =
                    TrimEnd(entities.ServiceProvider.FirstName) + " " + entities
                    .ServiceProvider.MiddleInitial + " " + TrimEnd
                    (entities.ServiceProvider.LastName);
                }
                else
                {
                  export.Group.Update.Question.Text50 = "";
                }

                var field6 = GetField(export.Group.Item.Answer, "text1");

                field6.Protected = true;

                var field7 = GetField(export.Group.Item.Question, "text50");

                field7.Color = "green";
                field7.Protected = true;

                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "";

                break;
              case 7:
                export.Group.Update.Question.Text50 =
                  "Is the NCP incarcerated?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.NcpIncarcerated ?? Spaces(1);
                export.Group.Update.Legend.Text3 = "Y/N";

                var field8 = GetField(export.Group.Item.Answer, "text1");

                field8.Highlighting = Highlighting.Underscore;
                field8.Protected = true;

                break;
              case 8:
                export.Group.Update.Question.Text50 =
                  "Was the income imputed for NCP?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.NcpIncImputed ?? Spaces(1);
                export.Group.Update.Legend.Text3 = "Y/N";

                var field9 = GetField(export.Group.Item.Answer, "text1");

                field9.Highlighting = Highlighting.Underscore;
                field9.Protected = true;

                break;
              case 9:
                export.Group.Update.Question.Text50 =
                  "Was the income imputed for CP?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.CpIncImputed ?? Spaces(1);
                export.Group.Update.Legend.Text3 = "Y/N";

                var field10 = GetField(export.Group.Item.Answer, "text1");

                field10.Highlighting = Highlighting.Underscore;
                field10.Protected = true;

                break;
              case 10:
                export.Group.Update.Question.Text50 =
                  "Is the CS worksheet the same";

                var field11 = GetField(export.Group.Item.Answer, "text1");

                field11.Protected = true;

                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "";

                break;
              case 11:
                export.Group.Update.Question.Text50 =
                  "   as filed with petition?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.CsWorksheetSame ?? Spaces(1);
                export.Group.Update.Legend.Text3 = "Y/N";

                var field12 = GetField(export.Group.Item.Answer, "text1");

                field12.Highlighting = Highlighting.Underscore;
                field12.Protected = true;

                break;
              case 12:
                export.Group.Update.Question.Text50 =
                  "Did the CS worksheet have";

                var field13 = GetField(export.Group.Item.Answer, "text1");

                field13.Protected = true;

                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "";

                break;
              case 13:
                export.Group.Update.Question.Text50 = "   any adjustments?";
                export.Group.Update.Answer.Text1 =
                  entities.GuidelineDeviations.CsWorksheetAdjustment ?? Spaces
                  (1);
                export.Group.Update.Legend.Text3 = "Y/N";

                var field14 = GetField(export.Group.Item.Answer, "text1");

                field14.Highlighting = Highlighting.Underscore;
                field14.Protected = true;

                break;
              default:
                export.Group.Update.Question.Text50 = "";

                var field15 = GetField(export.Group.Item.Answer, "text1");

                field15.Protected = true;

                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "";

                break;
            }
          }

          export.Group.CheckIndex();
          export.PageCount.Count = 1;
        }
        else
        {
          for(export.Group.Index = 0; export.Group.Index < Export
            .GroupGroup.Capacity; ++export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            ++local.Row.Count;

            switch(local.Row.Count)
            {
              case 1:
                export.Group.Update.Question.Text50 =
                  "Did NCP attend the hearing?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field1 = GetField(export.Group.Item.Answer, "text1");

                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = false;

                break;
              case 2:
                export.Group.Update.Question.Text50 =
                  "Did CP attend the hearing?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field2 = GetField(export.Group.Item.Answer, "text1");

                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;

                break;
              case 3:
                export.Group.Update.Question.Text50 =
                  "Did NCP attorney attend the hearing?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field3 = GetField(export.Group.Item.Answer, "text1");

                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = false;

                break;
              case 4:
                export.Group.Update.Question.Text50 =
                  "Did CP attorney attend the hearing?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field4 = GetField(export.Group.Item.Answer, "text1");

                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = false;

                break;
              case 5:
                export.Group.Update.Question.Text50 =
                  "Which IV-D attorney attended the hearing?";

                var field5 = GetField(export.Group.Item.Answer, "text1");

                field5.Protected = false;

                export.Group.Update.Legend.Text3 = "";
                export.Group.Update.Answer.Text1 = "+";

                break;
              case 6:
                export.Group.Update.Question.Text50 = "";

                var field6 = GetField(export.Group.Item.Answer, "text1");

                field6.Protected = true;

                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "";

                break;
              case 7:
                export.Group.Update.Question.Text50 =
                  "Is the NCP incarcerated?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field7 = GetField(export.Group.Item.Answer, "text1");

                field7.Highlighting = Highlighting.Underscore;
                field7.Protected = false;

                break;
              case 8:
                export.Group.Update.Question.Text50 =
                  "Was the income imputed for NCP?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field8 = GetField(export.Group.Item.Answer, "text1");

                field8.Highlighting = Highlighting.Underscore;
                field8.Protected = false;

                break;
              case 9:
                export.Group.Update.Question.Text50 =
                  "Was the income imputed for CP?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field9 = GetField(export.Group.Item.Answer, "text1");

                field9.Highlighting = Highlighting.Underscore;
                field9.Protected = false;

                break;
              case 10:
                export.Group.Update.Question.Text50 =
                  "Is the CS worksheet the same";

                var field10 = GetField(export.Group.Item.Answer, "text1");

                field10.Protected = true;

                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "";

                break;
              case 11:
                export.Group.Update.Question.Text50 =
                  "   as filed with petition?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field11 = GetField(export.Group.Item.Answer, "text1");

                field11.Highlighting = Highlighting.Underscore;
                field11.Protected = false;

                break;
              case 12:
                export.Group.Update.Question.Text50 =
                  "Did the CS worksheet have";

                var field12 = GetField(export.Group.Item.Answer, "text1");

                field12.Protected = true;

                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "";

                break;
              case 13:
                export.Group.Update.Question.Text50 = "   any adjustments?";
                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "Y/N";

                var field13 = GetField(export.Group.Item.Answer, "text1");

                field13.Highlighting = Highlighting.Underscore;
                field13.Protected = false;

                break;
              default:
                export.Group.Update.Question.Text50 = "";

                var field14 = GetField(export.Group.Item.Answer, "text1");

                field14.Protected = true;

                export.Group.Update.Answer.Text1 = "";
                export.Group.Update.Legend.Text3 = "";

                break;
            }
          }

          export.Group.CheckIndex();

          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          ExitState = "LEGAL_ACTION_NF";
        }

        break;
      case "PREV":
        if (export.PageCount.Count == 1)
        {
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        if (export.GuidelineDeviations.Identifier <= 0)
        {
          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        export.PageCount.Count = 1;

        for(export.Group.Index = 0; export.Group.Index < Export
          .GroupGroup.Capacity; ++export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          ++local.Row.Count;

          switch(local.Row.Count)
          {
            case 1:
              export.Group.Update.Question.Text50 =
                "Did NCP attend the hearing?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.NcpHearing;
              export.Group.Update.Legend.Text3 = "Y/N";

              var field1 = GetField(export.Group.Item.Answer, "text1");

              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;

              break;
            case 2:
              export.Group.Update.Question.Text50 =
                "Did CP attend the hearing?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.CpHearing ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field2 = GetField(export.Group.Item.Answer, "text1");

              field2.Highlighting = Highlighting.Underscore;
              field2.Protected = false;

              break;
            case 3:
              export.Group.Update.Question.Text50 =
                "Did NCP attorney attend the hearing?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.NcpAttorney ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field3 = GetField(export.Group.Item.Answer, "text1");

              field3.Highlighting = Highlighting.Underscore;
              field3.Protected = false;

              break;
            case 4:
              export.Group.Update.Question.Text50 =
                "Did CP attorney attend the hearing?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.CpAttorney ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field4 = GetField(export.Group.Item.Answer, "text1");

              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;

              break;
            case 5:
              export.Group.Update.Question.Text50 =
                "Which IV-D attorney attended the hearing?";

              var field5 = GetField(export.Group.Item.Answer, "text1");

              field5.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            case 6:
              if (ReadServiceProvider2())
              {
                export.Group.Update.Question.Text50 =
                  TrimEnd(entities.ServiceProvider.FirstName) + " " + entities
                  .ServiceProvider.MiddleInitial + " " + TrimEnd
                  (entities.ServiceProvider.LastName);
              }
              else
              {
                export.Group.Update.Question.Text50 = "";
              }

              var field6 = GetField(export.Group.Item.Question, "text50");

              field6.Color = "green";
              field6.Protected = true;

              export.Group.Update.Answer.Text1 = "";

              var field7 = GetField(export.Group.Item.Answer, "text1");

              field7.Protected = true;

              export.Group.Update.Legend.Text3 = "";

              break;
            case 7:
              export.Group.Update.Question.Text50 = "Is the NCP incarcerated?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.NcpIncarcerated ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field8 = GetField(export.Group.Item.Answer, "text1");

              field8.Highlighting = Highlighting.Underscore;
              field8.Protected = false;

              break;
            case 8:
              export.Group.Update.Question.Text50 =
                "Was the income imputed for NCP?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.NcpIncImputed ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field9 = GetField(export.Group.Item.Answer, "text1");

              field9.Highlighting = Highlighting.Underscore;
              field9.Protected = false;

              break;
            case 9:
              export.Group.Update.Question.Text50 =
                "Was the income imputed for CP?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.CpIncImputed ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field10 = GetField(export.Group.Item.Answer, "text1");

              field10.Highlighting = Highlighting.Underscore;
              field10.Protected = false;

              break;
            case 10:
              export.Group.Update.Question.Text50 =
                "Is the CS worksheet the same";
              export.Group.Update.Answer.Text1 = "";

              var field11 = GetField(export.Group.Item.Answer, "text1");

              field11.Protected = true;

              export.Group.Update.Legend.Text3 = "";

              break;
            case 11:
              export.Group.Update.Question.Text50 =
                "   as filed with petition?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.CsWorksheetSame ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field12 = GetField(export.Group.Item.Answer, "text1");

              field12.Highlighting = Highlighting.Underscore;
              field12.Protected = false;

              break;
            case 12:
              export.Group.Update.Question.Text50 = "Did the CS worksheet have";
              export.Group.Update.Answer.Text1 = "";

              var field13 = GetField(export.Group.Item.Answer, "text1");

              field13.Protected = true;

              export.Group.Update.Legend.Text3 = "";

              break;
            case 13:
              export.Group.Update.Question.Text50 = "   any adjustments?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.CsWorksheetAdjustment ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field14 = GetField(export.Group.Item.Answer, "text1");

              field14.Highlighting = Highlighting.Underscore;
              field14.Protected = false;

              break;
            default:
              export.Group.Update.Answer.Text1 = "";

              var field15 = GetField(export.Group.Item.Answer, "text1");

              field15.Protected = true;

              export.Group.Update.Question.Text50 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
          }
        }

        export.Group.CheckIndex();

        break;
      case "NEXT":
        if (IsEmpty(export.GuidelineDeviations.FinancialConditionAdjustment))
        {
          ExitState = "THERE_NO_MORE_RECORDS_TO_DISPLAY";

          return;
        }

        if (export.PageCount.Count == 2)
        {
          ExitState = "FN0000_BOTTOM_LIST_RETURN_TO_TOP";

          return;
        }

        if (export.GuidelineDeviations.Identifier <= 0)
        {
          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        export.PageCount.Count = 2;

        for(export.Group.Index = 0; export.Group.Index < Export
          .GroupGroup.Capacity; ++export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          ++local.Row.Count;

          switch(local.Row.Count)
          {
            case 1:
              export.Group.Update.Question.Text50 =
                "Was the low-income adjustment used?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.LowIncomeAdjustment ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field1 = GetField(export.Group.Item.Answer, "text1");

              field1.Highlighting = Highlighting.Underscore;
              field1.Protected = false;

              break;
            case 2:
              export.Group.Update.Question.Text50 =
                "Was the long-distance parenting";

              var field2 = GetField(export.Group.Item.Answer, "text1");

              field2.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            case 3:
              export.Group.Update.Question.Text50 =
                "     time cost adjusted used?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.LongDistanceAdjustment ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field3 = GetField(export.Group.Item.Answer, "text1");

              field3.Highlighting = Highlighting.Underscore;
              field3.Protected = false;

              break;
            case 4:
              export.Group.Update.Question.Text50 =
                "Was the parenting time adjusted used?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.ParentTimeAdjustment ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field4 = GetField(export.Group.Item.Answer, "text1");

              field4.Highlighting = Highlighting.Underscore;
              field4.Protected = false;

              break;
            case 5:
              export.Group.Update.Question.Text50 =
                "Was the income tax consideration";

              var field5 = GetField(export.Group.Item.Answer, "text1");

              field5.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            case 6:
              export.Group.Update.Question.Text50 = "     adjustment used?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.IncomeTaxAdjustment ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field6 = GetField(export.Group.Item.Answer, "text1");

              field6.Highlighting = Highlighting.Underscore;
              field6.Protected = false;

              break;
            case 7:
              export.Group.Update.Question.Text50 =
                "Was the special needs adjustment used?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.SpecialNeedsAdjustment ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field7 = GetField(export.Group.Item.Answer, "text1");

              field7.Highlighting = Highlighting.Underscore;
              field7.Protected = false;

              break;
            case 8:
              export.Group.Update.Question.Text50 =
                "Was the support past minority";

              var field8 = GetField(export.Group.Item.Answer, "text1");

              field8.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            case 9:
              export.Group.Update.Question.Text50 = "     adjustment used?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.MinorityAdjustment ?? Spaces(1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field9 = GetField(export.Group.Item.Answer, "text1");

              field9.Highlighting = Highlighting.Underscore;
              field9.Protected = false;

              break;
            case 10:
              export.Group.Update.Question.Text50 =
                "Was the overall financial condition";

              var field10 = GetField(export.Group.Item.Answer, "text1");

              field10.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            case 11:
              export.Group.Update.Question.Text50 = "     adjustment used?";
              export.Group.Update.Answer.Text1 =
                export.GuidelineDeviations.FinancialConditionAdjustment ?? Spaces
                (1);
              export.Group.Update.Legend.Text3 = "Y/N";

              var field11 = GetField(export.Group.Item.Answer, "text1");

              field11.Highlighting = Highlighting.Underscore;
              field11.Protected = false;

              break;
            case 12:
              export.Group.Update.Question.Text50 = "";

              var field12 = GetField(export.Group.Item.Answer, "text1");

              field12.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            case 13:
              export.Group.Update.Question.Text50 = "";

              var field13 = GetField(export.Group.Item.Answer, "text1");

              field13.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
            default:
              export.Group.Update.Question.Text50 = "";

              var field14 = GetField(export.Group.Item.Answer, "text1");

              field14.Protected = true;

              export.Group.Update.Answer.Text1 = "";
              export.Group.Update.Legend.Text3 = "";

              break;
          }
        }

        export.Group.CheckIndex();

        break;
      case "ADD":
        if (AsChar(import.FromLact.Text1) == 'Y')
        {
        }
        else
        {
          ExitState = "ADD_ONLY_FROM_LACT";

          return;

          // can only add a record when it comes from the lact screen
        }

        if (!ReadLegalAction())
        {
          ExitState = "LEGAL_ACTION_NF";

          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          return;
        }

        if (ReadGuidelineDeviations())
        {
          ExitState = "GUIDELINE_DEVIATIONS_AE";

          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          return;
        }

        if (export.PageCount.Count == 1)
        {
          local.Row.Count = 0;

          for(export.Group.Index = 0; export.Group.Index < Export
            .GroupGroup.Capacity; ++export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            ++local.Row.Count;

            switch(local.Row.Count)
            {
              case 1:
                var field1 = GetField(export.Group.Item.Answer, "text1");

                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.NcpHearing =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 2:
                var field2 = GetField(export.Group.Item.Answer, "text1");

                field2.Highlighting = Highlighting.Underscore;
                field2.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.CpHearing =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 3:
                var field3 = GetField(export.Group.Item.Answer, "text1");

                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.NcpAttorney =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 4:
                var field4 = GetField(export.Group.Item.Answer, "text1");

                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.CpAttorney =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 5:
                var field5 = GetField(export.Group.Item.Answer, "text1");

                field5.Protected = true;

                break;
              case 6:
                if (IsEmpty(export.Group.Item.Question.Text50))
                {
                  ExitState = "MUST_ENTER_IV_D_ATTORNEY";

                  var field14 = GetField(export.Group.Item.Question, "text50");

                  field14.Error = true;

                  // back up and set it up to go to svpl
                  --export.Group.Index;
                  export.Group.CheckSize();

                  export.Group.Update.Answer.Text1 = "S";

                  var field15 = GetField(export.Group.Item.Answer, "text1");

                  field15.Highlighting = Highlighting.Underscore;
                  field15.Protected = false;
                  field15.Focused = true;

                  return;
                }

                var field6 = GetField(export.Group.Item.Answer, "text1");

                field6.Protected = true;
                field6.Focused = false;

                break;
              case 7:
                var field7 = GetField(export.Group.Item.Answer, "text1");

                field7.Highlighting = Highlighting.Underscore;
                field7.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.NcpIncarcerated =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 8:
                var field8 = GetField(export.Group.Item.Answer, "text1");

                field8.Highlighting = Highlighting.Underscore;
                field8.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.NcpIncImputed =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 9:
                var field9 = GetField(export.Group.Item.Answer, "text1");

                field9.Highlighting = Highlighting.Underscore;
                field9.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.CpIncImputed =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 10:
                var field10 = GetField(export.Group.Item.Answer, "text1");

                field10.Protected = true;
                field10.Focused = false;

                break;
              case 11:
                var field11 = GetField(export.Group.Item.Answer, "text1");

                field11.Highlighting = Highlighting.Underscore;
                field11.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.CsWorksheetSame =
                    export.Group.Item.Answer.Text1;

                  if (AsChar(export.Group.Item.Answer.Text1) == 'N')
                  {
                    if (ReadCsePersonSupportWorksheetChildSupportWorksheet())
                    {
                      if ((!Lt(
                        entities.CsePersonSupportWorksheet.LastUpdatedTimestamp,
                        local.Compare.Timestamp) || !
                        Lt(entities.ChildSupportWorksheet.LastUpdatedTimestamp,
                        local.Compare.Timestamp)) && !
                        Lt(local.Null1.Timestamp, export.CswlReturn.Timestamp) ||
                        !
                        Lt(entities.CsePersonSupportWorksheet.
                          LastUpdatedTimestamp, export.CswlReturn.Timestamp) &&
                        Lt
                        (local.Null1.Timestamp, export.CswlReturn.Timestamp) ||
                        !
                        Lt(entities.ChildSupportWorksheet.LastUpdatedTimestamp,
                        export.CswlReturn.Timestamp) && Lt
                        (local.Null1.Timestamp, export.CswlReturn.Timestamp))
                      {
                        local.SkipCswl.Flag = "Y";
                      }
                    }

                    if (AsChar(local.SkipCswl.Flag) == 'Y')
                    {
                    }
                    else
                    {
                      export.CswlReturn.Timestamp = Now();
                      export.FromGldv.Text1 = "Y";
                      ExitState = "ECO_LNK_TO_CSWL";

                      return;
                    }
                  }
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 12:
                var field12 = GetField(export.Group.Item.Answer, "text1");

                field12.Protected = true;
                field12.Focused = false;

                break;
              case 13:
                var field13 = GetField(export.Group.Item.Answer, "text1");

                field13.Highlighting = Highlighting.Underscore;
                field13.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  if (AsChar(export.Group.Item.Answer.Text1) == 'Y')
                  {
                    local.Adjustments.Flag = "Y";
                  }

                  export.GuidelineDeviations.CsWorksheetAdjustment =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              default:
                break;
            }
          }

          export.Group.CheckIndex();

          if (local.Error.Count > 0)
          {
            ExitState = "MUST_ANSWER";

            return;
          }

          if (AsChar(local.Adjustments.Flag) == 'Y')
          {
            local.Row.Count = 0;

            // save data for add, set questions for page 2
            for(export.Group.Index = 0; export.Group.Index < Export
              .GroupGroup.Capacity; ++export.Group.Index)
            {
              if (!export.Group.CheckSize())
              {
                break;
              }

              ++local.Row.Count;

              switch(local.Row.Count)
              {
                case 1:
                  export.Group.Update.Question.Text50 =
                    "Was the low-income adjustment used?";
                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "Y/N";

                  var field1 = GetField(export.Group.Item.Answer, "text1");

                  field1.Highlighting = Highlighting.Underscore;
                  field1.Protected = false;

                  break;
                case 2:
                  export.Group.Update.Question.Text50 =
                    "Was the long-distance parenting";

                  var field2 = GetField(export.Group.Item.Answer, "text1");

                  field2.Protected = true;

                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "";

                  break;
                case 3:
                  export.Group.Update.Question.Text50 =
                    "     time cost adjusted used?";
                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "Y/N";

                  var field3 = GetField(export.Group.Item.Answer, "text1");

                  field3.Highlighting = Highlighting.Underscore;
                  field3.Protected = false;

                  break;
                case 4:
                  export.Group.Update.Question.Text50 =
                    "Was the parenting time adjusted used?";
                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "Y/N";

                  var field4 = GetField(export.Group.Item.Answer, "text1");

                  field4.Highlighting = Highlighting.Underscore;
                  field4.Protected = false;

                  break;
                case 5:
                  export.Group.Update.Question.Text50 =
                    "Was the income tax consideration";

                  var field5 = GetField(export.Group.Item.Answer, "text1");

                  field5.Protected = true;

                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "";

                  break;
                case 6:
                  export.Group.Update.Question.Text50 = "     adjustment used?";
                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "Y/N";

                  var field6 = GetField(export.Group.Item.Answer, "text1");

                  field6.Highlighting = Highlighting.Underscore;
                  field6.Protected = false;

                  break;
                case 7:
                  export.Group.Update.Question.Text50 =
                    "Was the special needs adjustment used?";
                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "Y/N";

                  var field7 = GetField(export.Group.Item.Answer, "text1");

                  field7.Highlighting = Highlighting.Underscore;
                  field7.Protected = false;

                  break;
                case 8:
                  export.Group.Update.Question.Text50 =
                    "Was the support past minority";

                  var field8 = GetField(export.Group.Item.Answer, "text1");

                  field8.Protected = true;

                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "";

                  break;
                case 9:
                  export.Group.Update.Question.Text50 = "     adjustment used?";
                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "Y/N";

                  var field9 = GetField(export.Group.Item.Answer, "text1");

                  field9.Highlighting = Highlighting.Underscore;
                  field9.Protected = false;

                  break;
                case 10:
                  export.Group.Update.Question.Text50 =
                    "Was the overall financial condition";

                  var field10 = GetField(export.Group.Item.Answer, "text1");

                  field10.Protected = true;

                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "";

                  break;
                case 11:
                  export.Group.Update.Question.Text50 = "     adjustment used?";
                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "Y/N";

                  var field11 = GetField(export.Group.Item.Answer, "text1");

                  field11.Highlighting = Highlighting.Underscore;
                  field11.Protected = false;

                  break;
                case 12:
                  export.Group.Update.Question.Text50 = "";

                  var field12 = GetField(export.Group.Item.Answer, "text1");

                  field12.Protected = true;

                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "";

                  break;
                case 13:
                  export.Group.Update.Question.Text50 = "";

                  var field13 = GetField(export.Group.Item.Answer, "text1");

                  field13.Protected = true;

                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "";

                  break;
                default:
                  export.Group.Update.Question.Text50 = "";

                  var field14 = GetField(export.Group.Item.Answer, "text1");

                  field14.Protected = true;

                  export.Group.Update.Answer.Text1 = "";
                  export.Group.Update.Legend.Text3 = "";

                  break;
              }
            }

            export.Group.CheckIndex();
            export.PageCount.Count = 2;
          }
          else
          {
            local.ControlTable.Identifier = "GUIDELINES DEVIATION";
            UseAccessControlTable();
            export.GuidelineDeviations.Identifier =
              local.ControlTable.LastUsedNumber;

            // add records
            try
            {
              CreateGuidelineDeviations();
              ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "GUIDELINE_DEVIATIONS_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "GUIDELINE_DEVIATIONS_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          return;
        }

        if (export.PageCount.Count == 2)
        {
          local.Row.Count = 0;

          for(export.Group.Index = 0; export.Group.Index < Export
            .GroupGroup.Capacity; ++export.Group.Index)
          {
            if (!export.Group.CheckSize())
            {
              break;
            }

            ++local.Row.Count;

            switch(local.Row.Count)
            {
              case 1:
                var field1 = GetField(export.Group.Item.Answer, "text1");

                field1.Highlighting = Highlighting.Underscore;
                field1.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.LowIncomeAdjustment =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 2:
                var field2 = GetField(export.Group.Item.Answer, "text1");

                field2.Protected = true;

                break;
              case 3:
                var field3 = GetField(export.Group.Item.Answer, "text1");

                field3.Highlighting = Highlighting.Underscore;
                field3.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.LongDistanceAdjustment =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 4:
                var field4 = GetField(export.Group.Item.Answer, "text1");

                field4.Highlighting = Highlighting.Underscore;
                field4.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.ParentTimeAdjustment =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 5:
                export.Group.Update.Answer.Text1 = "";

                var field5 = GetField(export.Group.Item.Answer, "text1");

                field5.Protected = true;

                break;
              case 6:
                var field6 = GetField(export.Group.Item.Answer, "text1");

                field6.Highlighting = Highlighting.Underscore;
                field6.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.IncomeTaxAdjustment =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 7:
                var field7 = GetField(export.Group.Item.Answer, "text1");

                field7.Highlighting = Highlighting.Underscore;
                field7.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.SpecialNeedsAdjustment =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 8:
                var field8 = GetField(export.Group.Item.Answer, "text1");

                field8.Protected = true;

                export.Group.Update.Answer.Text1 = "";

                break;
              case 9:
                var field9 = GetField(export.Group.Item.Answer, "text1");

                field9.Highlighting = Highlighting.Underscore;
                field9.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.MinorityAdjustment =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 10:
                var field10 = GetField(export.Group.Item.Answer, "text1");

                field10.Protected = true;

                export.Group.Update.Answer.Text1 = "";

                break;
              case 11:
                var field11 = GetField(export.Group.Item.Answer, "text1");

                field11.Highlighting = Highlighting.Underscore;
                field11.Protected = false;

                if (IsEmpty(export.Group.Item.Answer.Text1))
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ++local.Error.Count;
                }
                else if (AsChar(export.Group.Item.Answer.Text1) == 'Y' || AsChar
                  (export.Group.Item.Answer.Text1) == 'N')
                {
                  export.GuidelineDeviations.FinancialConditionAdjustment =
                    export.Group.Item.Answer.Text1;
                }
                else
                {
                  var field = GetField(export.Group.Item.Answer, "text1");

                  field.Error = true;

                  ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

                  return;
                }

                break;
              case 12:
                export.Group.Update.Question.Text50 = "";

                var field12 = GetField(export.Group.Item.Answer, "text1");

                field12.Protected = true;

                export.Group.Update.Answer.Text1 = "";

                break;
              case 13:
                export.Group.Update.Question.Text50 = "";

                var field13 = GetField(export.Group.Item.Answer, "text1");

                field13.Protected = true;

                export.Group.Update.Answer.Text1 = "";

                break;
              default:
                export.Group.Update.Question.Text50 = "";

                var field14 = GetField(export.Group.Item.Answer, "text1");

                field14.Protected = true;

                export.Group.Update.Answer.Text1 = "";

                break;
            }
          }

          export.Group.CheckIndex();

          if (local.Error.Count > 0)
          {
            ExitState = "MUST_ANSWER";

            return;
          }

          local.ControlTable.Identifier = "GUIDELINES DEVIATION";
          UseAccessControlTable();
          export.GuidelineDeviations.Identifier =
            local.ControlTable.LastUsedNumber;

          // add records
          try
          {
            CreateGuidelineDeviations();
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "GUIDELINE_DEVIATIONS_AE";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "GUIDELINE_DEVIATIONS_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

        break;
      case "RETURN":
        if (AsChar(export.FromLact.Text1) == 'Y')
        {
          ExitState = "ACO_NE0000_RETURN";
        }
        else if (AsChar(export.FromLacs.Text1) == 'Y')
        {
          ExitState = "ACO_NE0000_RETURN_NM";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void CreateGuidelineDeviations()
  {
    var identifier = export.GuidelineDeviations.Identifier;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var ncpHearing = export.GuidelineDeviations.NcpHearing;
    var cpHearing = export.GuidelineDeviations.CpHearing ?? "";
    var ncpAttorney = export.GuidelineDeviations.NcpAttorney ?? "";
    var cpAttorney = export.GuidelineDeviations.CpAttorney ?? "";
    var ivDAttorney =
      export.GuidelineDeviations.IvDAttorney.GetValueOrDefault();
    var ncpIncarcerated = export.GuidelineDeviations.NcpIncarcerated ?? "";
    var ncpIncImputed = export.GuidelineDeviations.NcpIncImputed ?? "";
    var cpIncImputed = export.GuidelineDeviations.CpIncImputed ?? "";
    var csWorksheetSame = export.GuidelineDeviations.CsWorksheetSame ?? "";
    var csWorksheetAdjustment =
      export.GuidelineDeviations.CsWorksheetAdjustment ?? "";
    var lowIncomeAdjustment =
      export.GuidelineDeviations.LowIncomeAdjustment ?? "";
    var longDistanceAdjustment =
      export.GuidelineDeviations.LongDistanceAdjustment ?? "";
    var parentTimeAdjustment =
      export.GuidelineDeviations.ParentTimeAdjustment ?? "";
    var incomeTaxAdjustment =
      export.GuidelineDeviations.IncomeTaxAdjustment ?? "";
    var specialNeedsAdjustment =
      export.GuidelineDeviations.SpecialNeedsAdjustment ?? "";
    var minorityAdjustment = export.GuidelineDeviations.MinorityAdjustment ?? ""
      ;
    var financialConditionAdjustment =
      export.GuidelineDeviations.FinancialConditionAdjustment ?? "";
    var extra1 = export.GuidelineDeviations.Extra1 ?? "";
    var extra2 = export.GuidelineDeviations.Extra2 ?? "";
    var fkCktLegalAclegalActionId = entities.LegalAction.Identifier;
    var cseCaseNumber = export.LactCase.Number;
    var csePersonNumber = export.LactCsePerson.Number;

    entities.GuidelineDeviations.Populated = false;
    Update("CreateGuidelineDeviations",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "ncpHearing", ncpHearing);
        db.SetNullableString(command, "cpHearing", cpHearing);
        db.SetNullableString(command, "ncpAttorney", ncpAttorney);
        db.SetNullableString(command, "cpAttorney", cpAttorney);
        db.SetNullableInt32(command, "ivDAttorney", ivDAttorney);
        db.SetNullableString(command, "ncpIncarcerated", ncpIncarcerated);
        db.SetNullableString(command, "ncpIncImputed", ncpIncImputed);
        db.SetNullableString(command, "cpIncImputed", cpIncImputed);
        db.SetNullableString(command, "csWorksheetSame", csWorksheetSame);
        db.SetNullableString(command, "csWorksheetAdj", csWorksheetAdjustment);
        db.SetNullableString(command, "lowIncomeAdj", lowIncomeAdjustment);
        db.
          SetNullableString(command, "longDistanceAdj", longDistanceAdjustment);
          
        db.SetNullableString(command, "parentTimeAdj", parentTimeAdjustment);
        db.SetNullableString(command, "incomeTaxAdj", incomeTaxAdjustment);
        db.
          SetNullableString(command, "specialNeedsAdj", specialNeedsAdjustment);
          
        db.SetNullableString(command, "minorityAdj", minorityAdjustment);
        db.SetNullableString(
          command, "financialCondAdj", financialConditionAdjustment);
        db.SetNullableString(command, "extra1", extra1);
        db.SetNullableString(command, "extra2", extra2);
        db.SetInt32(command, "ckfk01738", fkCktLegalAclegalActionId);
        db.SetNullableString(command, "cseCaseNum", cseCaseNumber);
        db.SetNullableString(command, "csePersonNum", csePersonNumber);
      });

    entities.GuidelineDeviations.Identifier = identifier;
    entities.GuidelineDeviations.CreatedBy = createdBy;
    entities.GuidelineDeviations.CreatedTstamp = createdTstamp;
    entities.GuidelineDeviations.NcpHearing = ncpHearing;
    entities.GuidelineDeviations.CpHearing = cpHearing;
    entities.GuidelineDeviations.NcpAttorney = ncpAttorney;
    entities.GuidelineDeviations.CpAttorney = cpAttorney;
    entities.GuidelineDeviations.IvDAttorney = ivDAttorney;
    entities.GuidelineDeviations.NcpIncarcerated = ncpIncarcerated;
    entities.GuidelineDeviations.NcpIncImputed = ncpIncImputed;
    entities.GuidelineDeviations.CpIncImputed = cpIncImputed;
    entities.GuidelineDeviations.CsWorksheetSame = csWorksheetSame;
    entities.GuidelineDeviations.CsWorksheetAdjustment = csWorksheetAdjustment;
    entities.GuidelineDeviations.LowIncomeAdjustment = lowIncomeAdjustment;
    entities.GuidelineDeviations.LongDistanceAdjustment =
      longDistanceAdjustment;
    entities.GuidelineDeviations.ParentTimeAdjustment = parentTimeAdjustment;
    entities.GuidelineDeviations.IncomeTaxAdjustment = incomeTaxAdjustment;
    entities.GuidelineDeviations.SpecialNeedsAdjustment =
      specialNeedsAdjustment;
    entities.GuidelineDeviations.MinorityAdjustment = minorityAdjustment;
    entities.GuidelineDeviations.FinancialConditionAdjustment =
      financialConditionAdjustment;
    entities.GuidelineDeviations.Extra1 = extra1;
    entities.GuidelineDeviations.Extra2 = extra2;
    entities.GuidelineDeviations.FkCktLegalAclegalActionId =
      fkCktLegalAclegalActionId;
    entities.GuidelineDeviations.CseCaseNumber = cseCaseNumber;
    entities.GuidelineDeviations.CsePersonNumber = csePersonNumber;
    entities.GuidelineDeviations.Populated = true;
  }

  private bool ReadCsePersonSupportWorksheetChildSupportWorksheet()
  {
    entities.CsePersonSupportWorksheet.Populated = false;
    entities.ChildSupportWorksheet.Populated = false;

    return Read("ReadCsePersonSupportWorksheetChildSupportWorksheet",
      (db, command) =>
      {
        db.SetString(command, "croType", export.LactCaseRole.Type1);
        db.SetString(command, "casNumber", export.LactCase.Number);
        db.SetString(command, "cspNumber", export.LactCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonSupportWorksheet.CroIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonSupportWorksheet.Identifer = db.GetInt32(reader, 1);
        entities.CsePersonSupportWorksheet.LastUpdatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.CsePersonSupportWorksheet.CswIdentifier =
          db.GetInt64(reader, 3);
        entities.ChildSupportWorksheet.Identifier = db.GetInt64(reader, 3);
        entities.CsePersonSupportWorksheet.CspNumber = db.GetString(reader, 4);
        entities.CsePersonSupportWorksheet.CasNumber = db.GetString(reader, 5);
        entities.CsePersonSupportWorksheet.CroType = db.GetString(reader, 6);
        entities.CsePersonSupportWorksheet.CssGuidelineYr =
          db.GetInt32(reader, 7);
        entities.ChildSupportWorksheet.CsGuidelineYear = db.GetInt32(reader, 7);
        entities.ChildSupportWorksheet.LgaIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ChildSupportWorksheet.LastUpdatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CsePersonSupportWorksheet.Populated = true;
        entities.ChildSupportWorksheet.Populated = true;
        CheckValid<CsePersonSupportWorksheet>("CroType",
          entities.CsePersonSupportWorksheet.CroType);
      });
  }

  private bool ReadGuidelineDeviations()
  {
    entities.GuidelineDeviations.Populated = false;

    return Read("ReadGuidelineDeviations",
      (db, command) =>
      {
        db.SetInt32(command, "ckfk01738", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.GuidelineDeviations.Identifier = db.GetInt32(reader, 0);
        entities.GuidelineDeviations.CreatedBy =
          db.GetNullableString(reader, 1);
        entities.GuidelineDeviations.CreatedTstamp = db.GetDateTime(reader, 2);
        entities.GuidelineDeviations.NcpHearing = db.GetString(reader, 3);
        entities.GuidelineDeviations.CpHearing =
          db.GetNullableString(reader, 4);
        entities.GuidelineDeviations.NcpAttorney =
          db.GetNullableString(reader, 5);
        entities.GuidelineDeviations.CpAttorney =
          db.GetNullableString(reader, 6);
        entities.GuidelineDeviations.IvDAttorney =
          db.GetNullableInt32(reader, 7);
        entities.GuidelineDeviations.NcpIncarcerated =
          db.GetNullableString(reader, 8);
        entities.GuidelineDeviations.NcpIncImputed =
          db.GetNullableString(reader, 9);
        entities.GuidelineDeviations.CpIncImputed =
          db.GetNullableString(reader, 10);
        entities.GuidelineDeviations.CsWorksheetSame =
          db.GetNullableString(reader, 11);
        entities.GuidelineDeviations.CsWorksheetAdjustment =
          db.GetNullableString(reader, 12);
        entities.GuidelineDeviations.LowIncomeAdjustment =
          db.GetNullableString(reader, 13);
        entities.GuidelineDeviations.LongDistanceAdjustment =
          db.GetNullableString(reader, 14);
        entities.GuidelineDeviations.ParentTimeAdjustment =
          db.GetNullableString(reader, 15);
        entities.GuidelineDeviations.IncomeTaxAdjustment =
          db.GetNullableString(reader, 16);
        entities.GuidelineDeviations.SpecialNeedsAdjustment =
          db.GetNullableString(reader, 17);
        entities.GuidelineDeviations.MinorityAdjustment =
          db.GetNullableString(reader, 18);
        entities.GuidelineDeviations.FinancialConditionAdjustment =
          db.GetNullableString(reader, 19);
        entities.GuidelineDeviations.Extra1 = db.GetNullableString(reader, 20);
        entities.GuidelineDeviations.Extra2 = db.GetNullableString(reader, 21);
        entities.GuidelineDeviations.FkCktLegalAclegalActionId =
          db.GetInt32(reader, 22);
        entities.GuidelineDeviations.CseCaseNumber =
          db.GetNullableString(reader, 23);
        entities.GuidelineDeviations.CsePersonNumber =
          db.GetNullableString(reader, 24);
        entities.GuidelineDeviations.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.GuidelineDeviations.IvDAttorney.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          export.GuidelineDeviations.IvDAttorney.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Question.
      /// </summary>
      [JsonPropertyName("question")]
      public WorkArea Question
      {
        get => question ??= new();
        set => question = value;
      }

      /// <summary>
      /// A value of Answer.
      /// </summary>
      [JsonPropertyName("answer")]
      public WorkArea Answer
      {
        get => answer ??= new();
        set => answer = value;
      }

      /// <summary>
      /// A value of Att.
      /// </summary>
      [JsonPropertyName("att")]
      public ServiceProvider Att
      {
        get => att ??= new();
        set => att = value;
      }

      /// <summary>
      /// A value of Legend.
      /// </summary>
      [JsonPropertyName("legend")]
      public WorkArea Legend
      {
        get => legend ??= new();
        set => legend = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private WorkArea question;
      private WorkArea answer;
      private ServiceProvider att;
      private WorkArea legend;
    }

    /// <summary>
    /// A value of CswlReturn.
    /// </summary>
    [JsonPropertyName("cswlReturn")]
    public DateWorkArea CswlReturn
    {
      get => cswlReturn ??= new();
      set => cswlReturn = value;
    }

    /// <summary>
    /// A value of GuidelineDeviations.
    /// </summary>
    [JsonPropertyName("guidelineDeviations")]
    public GuidelineDeviations GuidelineDeviations
    {
      get => guidelineDeviations ??= new();
      set => guidelineDeviations = value;
    }

    /// <summary>
    /// A value of LactCaseRole.
    /// </summary>
    [JsonPropertyName("lactCaseRole")]
    public CaseRole LactCaseRole
    {
      get => lactCaseRole ??= new();
      set => lactCaseRole = value;
    }

    /// <summary>
    /// A value of LactCsePerson.
    /// </summary>
    [JsonPropertyName("lactCsePerson")]
    public CsePerson LactCsePerson
    {
      get => lactCsePerson ??= new();
      set => lactCsePerson = value;
    }

    /// <summary>
    /// A value of LactCase.
    /// </summary>
    [JsonPropertyName("lactCase")]
    public Case1 LactCase
    {
      get => lactCase ??= new();
      set => lactCase = value;
    }

    /// <summary>
    /// A value of PageCount.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public Common PageCount
    {
      get => pageCount ??= new();
      set => pageCount = value;
    }

    /// <summary>
    /// A value of CswlCheck.
    /// </summary>
    [JsonPropertyName("cswlCheck")]
    public WorkArea CswlCheck
    {
      get => cswlCheck ??= new();
      set => cswlCheck = value;
    }

    /// <summary>
    /// A value of FromLacs.
    /// </summary>
    [JsonPropertyName("fromLacs")]
    public WorkArea FromLacs
    {
      get => fromLacs ??= new();
      set => fromLacs = value;
    }

    /// <summary>
    /// A value of FromLact.
    /// </summary>
    [JsonPropertyName("fromLact")]
    public WorkArea FromLact
    {
      get => fromLact ??= new();
      set => fromLact = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of IvdAttorney.
    /// </summary>
    [JsonPropertyName("ivdAttorney")]
    public ServiceProvider IvdAttorney
    {
      get => ivdAttorney ??= new();
      set => ivdAttorney = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private DateWorkArea cswlReturn;
    private GuidelineDeviations guidelineDeviations;
    private CaseRole lactCaseRole;
    private CsePerson lactCsePerson;
    private Case1 lactCase;
    private Common pageCount;
    private WorkArea cswlCheck;
    private WorkArea fromLacs;
    private WorkArea fromLact;
    private Array<GroupGroup> group;
    private LegalAction legalAction;
    private ServiceProvider ivdAttorney;
    private Standard standard;
    private NextTranInfo hidden;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Question.
      /// </summary>
      [JsonPropertyName("question")]
      public WorkArea Question
      {
        get => question ??= new();
        set => question = value;
      }

      /// <summary>
      /// A value of Answer.
      /// </summary>
      [JsonPropertyName("answer")]
      public WorkArea Answer
      {
        get => answer ??= new();
        set => answer = value;
      }

      /// <summary>
      /// A value of Att.
      /// </summary>
      [JsonPropertyName("att")]
      public ServiceProvider Att
      {
        get => att ??= new();
        set => att = value;
      }

      /// <summary>
      /// A value of Legend.
      /// </summary>
      [JsonPropertyName("legend")]
      public WorkArea Legend
      {
        get => legend ??= new();
        set => legend = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private WorkArea question;
      private WorkArea answer;
      private ServiceProvider att;
      private WorkArea legend;
    }

    /// <summary>
    /// A value of FromGldv.
    /// </summary>
    [JsonPropertyName("fromGldv")]
    public WorkArea FromGldv
    {
      get => fromGldv ??= new();
      set => fromGldv = value;
    }

    /// <summary>
    /// A value of CswlReturn.
    /// </summary>
    [JsonPropertyName("cswlReturn")]
    public DateWorkArea CswlReturn
    {
      get => cswlReturn ??= new();
      set => cswlReturn = value;
    }

    /// <summary>
    /// A value of LactCaseRole.
    /// </summary>
    [JsonPropertyName("lactCaseRole")]
    public CaseRole LactCaseRole
    {
      get => lactCaseRole ??= new();
      set => lactCaseRole = value;
    }

    /// <summary>
    /// A value of LactCsePerson.
    /// </summary>
    [JsonPropertyName("lactCsePerson")]
    public CsePerson LactCsePerson
    {
      get => lactCsePerson ??= new();
      set => lactCsePerson = value;
    }

    /// <summary>
    /// A value of LactCase.
    /// </summary>
    [JsonPropertyName("lactCase")]
    public Case1 LactCase
    {
      get => lactCase ??= new();
      set => lactCase = value;
    }

    /// <summary>
    /// A value of GuidelineDeviations.
    /// </summary>
    [JsonPropertyName("guidelineDeviations")]
    public GuidelineDeviations GuidelineDeviations
    {
      get => guidelineDeviations ??= new();
      set => guidelineDeviations = value;
    }

    /// <summary>
    /// A value of PageCount.
    /// </summary>
    [JsonPropertyName("pageCount")]
    public Common PageCount
    {
      get => pageCount ??= new();
      set => pageCount = value;
    }

    /// <summary>
    /// A value of CswlCheck.
    /// </summary>
    [JsonPropertyName("cswlCheck")]
    public WorkArea CswlCheck
    {
      get => cswlCheck ??= new();
      set => cswlCheck = value;
    }

    /// <summary>
    /// A value of FromLacs.
    /// </summary>
    [JsonPropertyName("fromLacs")]
    public WorkArea FromLacs
    {
      get => fromLacs ??= new();
      set => fromLacs = value;
    }

    /// <summary>
    /// A value of FromLact.
    /// </summary>
    [JsonPropertyName("fromLact")]
    public WorkArea FromLact
    {
      get => fromLact ??= new();
      set => fromLact = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of IvdAttorney.
    /// </summary>
    [JsonPropertyName("ivdAttorney")]
    public ServiceProvider IvdAttorney
    {
      get => ivdAttorney ??= new();
      set => ivdAttorney = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private WorkArea fromGldv;
    private DateWorkArea cswlReturn;
    private CaseRole lactCaseRole;
    private CsePerson lactCsePerson;
    private Case1 lactCase;
    private GuidelineDeviations guidelineDeviations;
    private Common pageCount;
    private WorkArea cswlCheck;
    private WorkArea fromLacs;
    private WorkArea fromLact;
    private Array<GroupGroup> group;
    private LegalAction legalAction;
    private ServiceProvider ivdAttorney;
    private Standard standard;
    private NextTranInfo hidden;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public Common Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SkipCswl.
    /// </summary>
    [JsonPropertyName("skipCswl")]
    public Common SkipCswl
    {
      get => skipCswl ??= new();
      set => skipCswl = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public DateWorkArea Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of Adjustments.
    /// </summary>
    [JsonPropertyName("adjustments")]
    public Common Adjustments
    {
      get => adjustments ??= new();
      set => adjustments = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Row.
    /// </summary>
    [JsonPropertyName("row")]
    public Common Row
    {
      get => row ??= new();
      set => row = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    private Common ap;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common skipCswl;
    private DateWorkArea null1;
    private DateWorkArea compare;
    private Common adjustments;
    private Common error;
    private Common row;
    private TextWorkArea textWorkArea;
    private ControlTable controlTable;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of GuidelineDeviations.
    /// </summary>
    [JsonPropertyName("guidelineDeviations")]
    public GuidelineDeviations GuidelineDeviations
    {
      get => guidelineDeviations ??= new();
      set => guidelineDeviations = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonSupportWorksheet.
    /// </summary>
    [JsonPropertyName("csePersonSupportWorksheet")]
    public CsePersonSupportWorksheet CsePersonSupportWorksheet
    {
      get => csePersonSupportWorksheet ??= new();
      set => csePersonSupportWorksheet = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ChildSupportWorksheet.
    /// </summary>
    [JsonPropertyName("childSupportWorksheet")]
    public ChildSupportWorksheet ChildSupportWorksheet
    {
      get => childSupportWorksheet ??= new();
      set => childSupportWorksheet = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private GuidelineDeviations guidelineDeviations;
    private ServiceProvider serviceProvider;
    private CsePerson csePerson;
    private CsePersonSupportWorksheet csePersonSupportWorksheet;
    private CaseRole caseRole;
    private Case1 case1;
    private ChildSupportWorksheet childSupportWorksheet;
    private LegalAction legalAction;
  }
#endregion
}
