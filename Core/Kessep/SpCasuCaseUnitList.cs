// Program: SP_CASU_CASE_UNIT_LIST, ID: 372326755, model: 746.
// Short name: SWECASUP
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
/// A program: SP_CASU_CASE_UNIT_LIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SpCasuCaseUnitList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CASU_CASE_UNIT_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCasuCaseUnitList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCasuCaseUnitList.
  /// </summary>
  public SpCasuCaseUnitList(IContext context, Import import, Export export):
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
    // Date	  Developer	Request #	Description
    // 10/24/96 Regan Welborn          Initial Development
    // 05/28/97 R, Grey		Add select for ASIN logic
    // 05/29/97 R. Grey		Add Close Rsn Code to screen
    // 06/  /97 R. Grey		Rework 'Limbo' logic
    // 06/10/97 R. Grey		View match Security Cab
    // 				Enable display upon entry from Next Tran
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }
    else if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.Hidden.Assign(import.Hidden);
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.Case1.Number = import.Case1.Number;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CaseNumber))
      {
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base.  Set command to initial command required or ESCAPE
      // ---------------------------------------------
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "DELETE"))
    {
      UseScCabTestSecurity();
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.GrCommon.SelectChar =
          import.Group.Item.GrCommon.SelectChar;

        if (Equal(global.Command, "RETLINK"))
        {
          if (AsChar(import.Group.Item.GrCommon.SelectChar) == '*')
          {
            export.Group.Update.GrCommon.SelectChar = "";
          }

          global.Command = "DISPLAY";
        }

        export.Group.Update.GrCaseUnit.Assign(import.Group.Item.GrCaseUnit);
        export.Group.Update.GrApCsePerson.Number =
          import.Group.Item.GrApCsePerson.Number;
        export.Group.Update.GrApCsePersonsWorkSet.FormattedName =
          import.Group.Item.GrApCsePersonsWorkSet.FormattedName;
        export.Group.Update.GrArCsePerson.Number =
          import.Group.Item.GrArCsePerson.Number;
        export.Group.Update.GrArCsePersonsWorkSet.FormattedName =
          import.Group.Item.GrArCsePersonsWorkSet.FormattedName;
        export.Group.Update.GrChCsePerson.Number =
          import.Group.Item.GrChCsePerson.Number;
        export.Group.Update.GrChCsePersonsWorkSet.FormattedName =
          import.Group.Item.GrChCsePersonsWorkSet.FormattedName;
        export.Group.Update.GrCsunitSt1.Text1 =
          import.Group.Item.GrCsunitSt1.Text1;
        export.Group.Update.GrCsunitSt2.Text1 =
          import.Group.Item.GrCsunitSt2.Text1;
        export.Group.Update.GrCsunitSt3.Text1 =
          import.Group.Item.GrCsunitSt3.Text1;
        export.Group.Update.GrCsunitSt4.Text1 =
          import.Group.Item.GrCsunitSt4.Text1;
        export.Group.Update.GrCsunitSt5.Text1 =
          import.Group.Item.GrCsunitSt5.Text1;
        export.Group.Update.GrNbr.CuNumber = import.Group.Item.GrNbr.CuNumber;

        // ***********************************************
        // Maintain screen field highlighting for Case Units in a 'Limbo' 
        // status.
        // ***********************************************
        if (AsChar(export.Group.Item.GrCsunitSt2.Text1) == 'E')
        {
          // ************************************************
          // If Expedited Paternity (Service Type) Case:
          // And the AP is determined not to be the Father
          // ************************************************
          if (AsChar(export.Group.Item.GrCsunitSt4.Text1) == 'N')
          {
            var field1 = GetField(export.Group.Item.GrNbr, "cuNumber");

            field1.Color = "yellow";
            field1.Intensity = Intensity.High;
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.GrCsunitSt2, "text1");

            field2.Color = "yellow";
            field2.Intensity = Intensity.High;
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.GrCsunitSt4, "text1");

            field3.Color = "yellow";
            field3.Intensity = Intensity.High;
            field3.Protected = true;
          }

          // ************************************************
          // If Expedited Paternity (Service Type) Case:
          // And the AP IS determined to be the Father
          // ************************************************
          if (AsChar(export.Group.Item.GrCsunitSt4.Text1) == 'Y')
          {
            var field1 = GetField(export.Group.Item.GrNbr, "cuNumber");

            field1.Color = "yellow";
            field1.Intensity = Intensity.High;
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.GrCsunitSt2, "text1");

            field2.Color = "yellow";
            field2.Intensity = Intensity.High;
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.GrCsunitSt4, "text1");

            field3.Color = "yellow";
            field3.Intensity = Intensity.High;
            field3.Protected = true;
          }
        }

        if (AsChar(export.Group.Item.GrCsunitSt2.Text1) == 'F')
        {
          // ************************************************
          // If Full Service (Service Type) Case:
          // And the AP is determined not to have any outstanding Obligations
          // ************************************************
          if (AsChar(export.Group.Item.GrCsunitSt5.Text1) == 'N')
          {
            var field1 = GetField(export.Group.Item.GrNbr, "cuNumber");

            field1.Color = "yellow";
            field1.Intensity = Intensity.High;
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.GrCsunitSt2, "text1");

            field2.Color = "yellow";
            field2.Intensity = Intensity.High;
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.GrCsunitSt5, "text1");

            field3.Color = "yellow";
            field3.Intensity = Intensity.High;
            field3.Protected = true;
          }
        }

        if (AsChar(export.Group.Item.GrCsunitSt2.Text1) == 'L')
        {
          // ************************************************
          // If Locate Only (Service Type) Case:
          // And the AP Location is verified
          // ************************************************
          if (AsChar(export.Group.Item.GrCsunitSt3.Text1) == 'Y')
          {
            var field1 = GetField(export.Group.Item.GrNbr, "cuNumber");

            field1.Color = "yellow";
            field1.Intensity = Intensity.High;
            field1.Protected = true;

            var field2 = GetField(export.Group.Item.GrCsunitSt2, "text1");

            field2.Color = "yellow";
            field2.Intensity = Intensity.High;
            field2.Protected = true;

            var field3 = GetField(export.Group.Item.GrCsunitSt3, "text1");

            field3.Color = "yellow";
            field3.Intensity = Intensity.High;
            field3.Protected = true;
          }
        }

        if (Equal(export.Group.Item.GrCaseUnit.ClosureDate, local.Max.Date))
        {
          export.Group.Update.GrCaseUnit.ClosureDate = local.Initialized.Date;
        }

        export.Group.Next();
      }
    }

    if (Equal(global.Command, "ENTER"))
    {
      if (!IsEmpty(import.Standard.NextTransaction))
      {
        // ---------------------------------------------
        // User is going out of this screen to another
        // ---------------------------------------------
        UseScCabNextTranPut();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;
        }

        return;
      }

      ExitState = "ACO_NE0000_INVALID_PF_KEY";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (!IsEmpty(export.Case1.Number))
    {
      UseCabZeroFillNumber();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        UseSpCabReachCaseUnitForCase();

        if (IsExitState("CASE_NF"))
        {
          var field = GetField(export.Case1, "number");

          field.Error = true;

          ExitState = "SP0000_MUST_ENTER_CASE_NUMBER";
        }

        UseCabSetMaximumDiscontinueDate();

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          // ***********************************************
          // Set screen field highlighting for Case Units in a 'Limbo' status.
          // ***********************************************
          if (AsChar(export.Group.Item.GrCsunitSt2.Text1) == 'E')
          {
            // ************************************************
            // If Expedited Paternity (Service Type) Case:
            // And the AP is determined not to be the Father
            // ************************************************
            if (AsChar(export.Group.Item.GrCsunitSt4.Text1) == 'N')
            {
              var field1 = GetField(export.Group.Item.GrNbr, "cuNumber");

              field1.Color = "yellow";
              field1.Intensity = Intensity.High;
              field1.Protected = true;

              var field2 = GetField(export.Group.Item.GrCsunitSt2, "text1");

              field2.Color = "yellow";
              field2.Intensity = Intensity.High;
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.GrCsunitSt4, "text1");

              field3.Color = "yellow";
              field3.Intensity = Intensity.High;
              field3.Protected = true;
            }

            // ************************************************
            // If Expedited Paternity (Service Type) Case:
            // And the AP IS determined to be the Father
            // ************************************************
            if (AsChar(export.Group.Item.GrCsunitSt4.Text1) == 'Y')
            {
              var field1 = GetField(export.Group.Item.GrNbr, "cuNumber");

              field1.Color = "yellow";
              field1.Intensity = Intensity.High;
              field1.Protected = true;

              var field2 = GetField(export.Group.Item.GrCsunitSt2, "text1");

              field2.Color = "yellow";
              field2.Intensity = Intensity.High;
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.GrCsunitSt4, "text1");

              field3.Color = "yellow";
              field3.Intensity = Intensity.High;
              field3.Protected = true;
            }
          }

          if (AsChar(export.Group.Item.GrCsunitSt2.Text1) == 'F')
          {
            // ************************************************
            // If Full Service (Service Type) Case:
            // And the AP is determined not to have any outstanding Obligations
            // ************************************************
            if (AsChar(export.Group.Item.GrCsunitSt5.Text1) == 'N')
            {
              var field1 = GetField(export.Group.Item.GrNbr, "cuNumber");

              field1.Color = "yellow";
              field1.Intensity = Intensity.High;
              field1.Protected = true;

              var field2 = GetField(export.Group.Item.GrCsunitSt2, "text1");

              field2.Color = "yellow";
              field2.Intensity = Intensity.High;
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.GrCsunitSt5, "text1");

              field3.Color = "yellow";
              field3.Intensity = Intensity.High;
              field3.Protected = true;
            }
          }

          if (AsChar(export.Group.Item.GrCsunitSt2.Text1) == 'L')
          {
            // ************************************************
            // If Locate Only (Service Type) Case:
            // And the AP Location is verified
            // ************************************************
            if (AsChar(export.Group.Item.GrCsunitSt3.Text1) == 'Y')
            {
              var field1 = GetField(export.Group.Item.GrNbr, "cuNumber");

              field1.Color = "yellow";
              field1.Intensity = Intensity.High;
              field1.Protected = true;

              var field2 = GetField(export.Group.Item.GrCsunitSt2, "text1");

              field2.Color = "yellow";
              field2.Intensity = Intensity.High;
              field2.Protected = true;

              var field3 = GetField(export.Group.Item.GrCsunitSt3, "text1");

              field3.Color = "yellow";
              field3.Intensity = Intensity.High;
              field3.Protected = true;
            }
          }

          if (Equal(export.Group.Item.GrCaseUnit.ClosureDate, local.Max.Date))
          {
            export.Group.Update.GrCaseUnit.ClosureDate = local.Initialized.Date;
          }
        }

        if (export.Selected.CuNumber > 0)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (export.Group.Item.GrNbr.CuNumber == export.Selected.CuNumber)
            {
              export.Group.Update.GrCommon.SelectChar = "*";
            }
          }
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ASIN":
        export.PassHeaderObject.Text20 = "CASE UNIT FUNCTION";
        local.Common.Count = 0;
        local.EasyPrompt.Count = 0;

        export.Group.Index = 0;
        export.Group.Clear();

        for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
          import.Group.Index)
        {
          if (export.Group.IsFull)
          {
            break;
          }

          export.Group.Update.GrCommon.SelectChar =
            import.Group.Item.GrCommon.SelectChar;
          export.Group.Update.GrCaseUnit.Assign(import.Group.Item.GrCaseUnit);
          export.Group.Update.GrApCsePerson.Number =
            import.Group.Item.GrApCsePerson.Number;
          export.Group.Update.GrApCsePersonsWorkSet.FormattedName =
            import.Group.Item.GrApCsePersonsWorkSet.FormattedName;
          export.Group.Update.GrArCsePerson.Number =
            import.Group.Item.GrArCsePerson.Number;
          export.Group.Update.GrArCsePersonsWorkSet.FormattedName =
            import.Group.Item.GrArCsePersonsWorkSet.FormattedName;
          export.Group.Update.GrChCsePerson.Number =
            import.Group.Item.GrChCsePerson.Number;
          export.Group.Update.GrChCsePersonsWorkSet.FormattedName =
            import.Group.Item.GrChCsePersonsWorkSet.FormattedName;
          export.Group.Update.GrCsunitSt1.Text1 =
            import.Group.Item.GrCsunitSt1.Text1;
          export.Group.Update.GrCsunitSt2.Text1 =
            import.Group.Item.GrCsunitSt2.Text1;
          export.Group.Update.GrCsunitSt3.Text1 =
            import.Group.Item.GrCsunitSt3.Text1;
          export.Group.Update.GrCsunitSt4.Text1 =
            import.Group.Item.GrCsunitSt4.Text1;
          export.Group.Update.GrCsunitSt5.Text1 =
            import.Group.Item.GrCsunitSt5.Text1;
          export.Group.Update.GrNbr.CuNumber = import.Group.Item.GrNbr.CuNumber;

          if (import.Group.Item.GrNbr.CuNumber > 0)
          {
            ++local.EasyPrompt.Count;
          }

          if (AsChar(import.Group.Item.GrCommon.SelectChar) == 'S')
          {
            export.Group.Update.GrCommon.SelectChar = "*";
            export.Selected.CuNumber = import.Group.Item.GrNbr.CuNumber;
            ++local.Common.Count;
          }

          export.Group.Next();
        }

        switch(local.Common.Count)
        {
          case 0:
            if (local.EasyPrompt.Count == 1)
            {
              for(import.Group.Index = 0; import.Group.Index < import
                .Group.Count; ++import.Group.Index)
              {
                if (import.Group.Item.GrNbr.CuNumber > 0)
                {
                  export.Selected.CuNumber = import.Group.Item.GrNbr.CuNumber;
                }
              }

              ExitState = "ECO_LNK_TO_ASIN";
            }
            else
            {
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";
            }

            break;
          case 1:
            ExitState = "ECO_LNK_TO_ASIN";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveGroup(SpCabReachCaseUnitForCase.Export.
    GroupGroup source, Export.GroupGroup target)
  {
    target.GrCommon.SelectChar = source.GrCommon.SelectChar;
    target.GrNbr.CuNumber = source.GrNbr.CuNumber;
    target.GrCsunitSt1.Text1 = source.GrCsunitSt1.Text1;
    target.GrCsunitSt2.Text1 = source.GrCsunitSt2.Text1;
    target.GrCsunitSt3.Text1 = source.GrCsunitSt3.Text1;
    target.GrCsunitSt4.Text1 = source.GrCsunitSt4.Text1;
    target.GrCsunitSt5.Text1 = source.GrCsunitSt5.Text1;
    target.GrArCsePerson.Number = source.GrArCsePerson.Number;
    target.GrCaseUnit.Assign(source.GrCaseUnit);
    target.GrApCsePerson.Number = source.GrApCsePerson.Number;
    target.GrChCsePerson.Number = source.GrChCsePerson.Number;
    target.GrChCsePersonsWorkSet.FormattedName =
      source.GrChCsePersonsWorkSet.FormattedName;
    target.GrArCsePersonsWorkSet.FormattedName =
      source.GrArCsePersonsWorkSet.FormattedName;
    target.GrApCsePersonsWorkSet.FormattedName =
      source.GrApCsePersonsWorkSet.FormattedName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Case1.Number = useImport.Case1.Number;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSpCabReachCaseUnitForCase()
  {
    var useImport = new SpCabReachCaseUnitForCase.Import();
    var useExport = new SpCabReachCaseUnitForCase.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SpCabReachCaseUnitForCase.Execute, useImport, useExport);

    export.Case1.Number = useExport.Case1.Number;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    useExport.Group.CopyTo(export.Group, MoveGroup);
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
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>
      /// A value of GrNbr.
      /// </summary>
      [JsonPropertyName("grNbr")]
      public CaseUnit GrNbr
      {
        get => grNbr ??= new();
        set => grNbr = value;
      }

      /// <summary>
      /// A value of GrCsunitSt1.
      /// </summary>
      [JsonPropertyName("grCsunitSt1")]
      public WorkArea GrCsunitSt1
      {
        get => grCsunitSt1 ??= new();
        set => grCsunitSt1 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt2.
      /// </summary>
      [JsonPropertyName("grCsunitSt2")]
      public WorkArea GrCsunitSt2
      {
        get => grCsunitSt2 ??= new();
        set => grCsunitSt2 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt3.
      /// </summary>
      [JsonPropertyName("grCsunitSt3")]
      public WorkArea GrCsunitSt3
      {
        get => grCsunitSt3 ??= new();
        set => grCsunitSt3 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt4.
      /// </summary>
      [JsonPropertyName("grCsunitSt4")]
      public WorkArea GrCsunitSt4
      {
        get => grCsunitSt4 ??= new();
        set => grCsunitSt4 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt5.
      /// </summary>
      [JsonPropertyName("grCsunitSt5")]
      public WorkArea GrCsunitSt5
      {
        get => grCsunitSt5 ??= new();
        set => grCsunitSt5 = value;
      }

      /// <summary>
      /// A value of GrArCsePerson.
      /// </summary>
      [JsonPropertyName("grArCsePerson")]
      public CsePerson GrArCsePerson
      {
        get => grArCsePerson ??= new();
        set => grArCsePerson = value;
      }

      /// <summary>
      /// A value of GrCaseUnit.
      /// </summary>
      [JsonPropertyName("grCaseUnit")]
      public CaseUnit GrCaseUnit
      {
        get => grCaseUnit ??= new();
        set => grCaseUnit = value;
      }

      /// <summary>
      /// A value of GrApCsePerson.
      /// </summary>
      [JsonPropertyName("grApCsePerson")]
      public CsePerson GrApCsePerson
      {
        get => grApCsePerson ??= new();
        set => grApCsePerson = value;
      }

      /// <summary>
      /// A value of GrChCsePerson.
      /// </summary>
      [JsonPropertyName("grChCsePerson")]
      public CsePerson GrChCsePerson
      {
        get => grChCsePerson ??= new();
        set => grChCsePerson = value;
      }

      /// <summary>
      /// A value of GrChCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grChCsePersonsWorkSet")]
      public CsePersonsWorkSet GrChCsePersonsWorkSet
      {
        get => grChCsePersonsWorkSet ??= new();
        set => grChCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GrArCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grArCsePersonsWorkSet")]
      public CsePersonsWorkSet GrArCsePersonsWorkSet
      {
        get => grArCsePersonsWorkSet ??= new();
        set => grArCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GrApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grApCsePersonsWorkSet")]
      public CsePersonsWorkSet GrApCsePersonsWorkSet
      {
        get => grApCsePersonsWorkSet ??= new();
        set => grApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common grCommon;
      private CaseUnit grNbr;
      private WorkArea grCsunitSt1;
      private WorkArea grCsunitSt2;
      private WorkArea grCsunitSt3;
      private WorkArea grCsunitSt4;
      private WorkArea grCsunitSt5;
      private CsePerson grArCsePerson;
      private CaseUnit grCaseUnit;
      private CsePerson grApCsePerson;
      private CsePerson grChCsePerson;
      private CsePersonsWorkSet grChCsePersonsWorkSet;
      private CsePersonsWorkSet grArCsePersonsWorkSet;
      private CsePersonsWorkSet grApCsePersonsWorkSet;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private ServiceProvider serviceProvider;
    private Office office;
    private Array<GroupGroup> group;
    private Case1 case1;
    private Standard standard;
    private NextTranInfo hidden;
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
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>
      /// A value of GrNbr.
      /// </summary>
      [JsonPropertyName("grNbr")]
      public CaseUnit GrNbr
      {
        get => grNbr ??= new();
        set => grNbr = value;
      }

      /// <summary>
      /// A value of GrCsunitSt1.
      /// </summary>
      [JsonPropertyName("grCsunitSt1")]
      public WorkArea GrCsunitSt1
      {
        get => grCsunitSt1 ??= new();
        set => grCsunitSt1 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt2.
      /// </summary>
      [JsonPropertyName("grCsunitSt2")]
      public WorkArea GrCsunitSt2
      {
        get => grCsunitSt2 ??= new();
        set => grCsunitSt2 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt3.
      /// </summary>
      [JsonPropertyName("grCsunitSt3")]
      public WorkArea GrCsunitSt3
      {
        get => grCsunitSt3 ??= new();
        set => grCsunitSt3 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt4.
      /// </summary>
      [JsonPropertyName("grCsunitSt4")]
      public WorkArea GrCsunitSt4
      {
        get => grCsunitSt4 ??= new();
        set => grCsunitSt4 = value;
      }

      /// <summary>
      /// A value of GrCsunitSt5.
      /// </summary>
      [JsonPropertyName("grCsunitSt5")]
      public WorkArea GrCsunitSt5
      {
        get => grCsunitSt5 ??= new();
        set => grCsunitSt5 = value;
      }

      /// <summary>
      /// A value of GrArCsePerson.
      /// </summary>
      [JsonPropertyName("grArCsePerson")]
      public CsePerson GrArCsePerson
      {
        get => grArCsePerson ??= new();
        set => grArCsePerson = value;
      }

      /// <summary>
      /// A value of GrCaseUnit.
      /// </summary>
      [JsonPropertyName("grCaseUnit")]
      public CaseUnit GrCaseUnit
      {
        get => grCaseUnit ??= new();
        set => grCaseUnit = value;
      }

      /// <summary>
      /// A value of GrApCsePerson.
      /// </summary>
      [JsonPropertyName("grApCsePerson")]
      public CsePerson GrApCsePerson
      {
        get => grApCsePerson ??= new();
        set => grApCsePerson = value;
      }

      /// <summary>
      /// A value of GrChCsePerson.
      /// </summary>
      [JsonPropertyName("grChCsePerson")]
      public CsePerson GrChCsePerson
      {
        get => grChCsePerson ??= new();
        set => grChCsePerson = value;
      }

      /// <summary>
      /// A value of GrChCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grChCsePersonsWorkSet")]
      public CsePersonsWorkSet GrChCsePersonsWorkSet
      {
        get => grChCsePersonsWorkSet ??= new();
        set => grChCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GrArCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grArCsePersonsWorkSet")]
      public CsePersonsWorkSet GrArCsePersonsWorkSet
      {
        get => grArCsePersonsWorkSet ??= new();
        set => grArCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GrApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("grApCsePersonsWorkSet")]
      public CsePersonsWorkSet GrApCsePersonsWorkSet
      {
        get => grApCsePersonsWorkSet ??= new();
        set => grApCsePersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common grCommon;
      private CaseUnit grNbr;
      private WorkArea grCsunitSt1;
      private WorkArea grCsunitSt2;
      private WorkArea grCsunitSt3;
      private WorkArea grCsunitSt4;
      private WorkArea grCsunitSt5;
      private CsePerson grArCsePerson;
      private CaseUnit grCaseUnit;
      private CsePerson grApCsePerson;
      private CsePerson grChCsePerson;
      private CsePersonsWorkSet grChCsePersonsWorkSet;
      private CsePersonsWorkSet grArCsePersonsWorkSet;
      private CsePersonsWorkSet grApCsePersonsWorkSet;
    }

    /// <summary>
    /// A value of PassHeaderObject.
    /// </summary>
    [JsonPropertyName("passHeaderObject")]
    public SpTextWorkArea PassHeaderObject
    {
      get => passHeaderObject ??= new();
      set => passHeaderObject = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CaseUnit Selected
    {
      get => selected ??= new();
      set => selected = value;
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

    private SpTextWorkArea passHeaderObject;
    private Case1 case1;
    private Array<GroupGroup> group;
    private ServiceProvider serviceProvider;
    private Office office;
    private NextTranInfo hidden;
    private CaseUnit selected;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of EasyPrompt.
    /// </summary>
    [JsonPropertyName("easyPrompt")]
    public Common EasyPrompt
    {
      get => easyPrompt ??= new();
      set => easyPrompt = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common easyPrompt;
    private DateWorkArea max;
    private DateWorkArea initialized;
    private NextTranInfo nextTranInfo;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private CaseUnit caseUnit;
  }
#endregion
}
