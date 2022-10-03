// Program: OE_EXAL_EXTERNAL_AGENCY_LIST, ID: 371814985, model: 746.
// Short name: SWEEXALP
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
/// A program: OE_EXAL_EXTERNAL_AGENCY_LIST.
/// </para>
/// <para>
/// Resp: Legal
/// This procedure is used to List and Select EXTERNAL AGENCY sorted either by 
/// Number, Name, City, or Type. Optionally the User can select a Specific
/// EXTERNAL AGENCY for Return to Calling Procedures.	
/// The List produced will list multiple addresses for EXTERNAL AGENCYs 
/// selected.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeExalExternalAgencyList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EXAL_EXTERNAL_AGENCY_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeExalExternalAgencyList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeExalExternalAgencyList.
  /// </summary>
  public OeExalExternalAgencyList(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // T.O.Redmond	01/02/96	Initial Code
    // T.O.Redmond	02/13/96	Retrofit
    // G.Lofton	03/03/96	Made corrections
    // to problems encountered during unit test.
    // R. Marchman     11/08/96        Add new security and next tran.
    // ******** END MAINTENANCE LOG ****************
    // This Procedure will display a list of
    // EXTERNAL_AGENCY with multiple addresses
    // depending upon user selection.
    // User may select to produce the display sorted
    // either by City, Number, Name, or Type.
    // User may select a row from the display for
    // return to Calling Procedures.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // Move Imports to Exports.
    export.StartingExternalAgency.Assign(import.StartingExternalAgency);
    MoveExternalAgencyAddress(import.StartingExternalAgencyAddress,
      export.StartingExternalAgencyAddress);
    export.SortBy.SelectChar = import.SortBy.SelectChar;

    // -----------------------------------------------------
    // Beginning Of Change
    // New Prompt field added.
    // -----------------------------------------------------
    export.Prompt.Text1 = import.Prompt.Text1;

    // -----------------------------------------------------
    // End Of Change
    // New Prompt field added.
    // -----------------------------------------------------
    export.AgencyTypeDesc.Description = import.AgencyTypeDesc.Description;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      ExitState = "ZD_ACO_NI000_SEARCH_CRITERIA_REQ";

      return;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.AgencyExternalAgency.Assign(
        import.Group.Item.AgencyExternalAgency);
      MoveExternalAgencyAddress(import.Group.Item.AgencyExternalAgencyAddress,
        export.Group.Update.AgencyExternalAgencyAddress);

      if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
      {
        ++local.Count.Count;
        export.Selected.Assign(export.Group.Item.AgencyExternalAgency);
      }

      // ---------------------------------
      // Beginning Of change TC # 52
      // --------------------------------
      if (AsChar(export.Group.Item.Common.SelectChar) != 'S' && !
        IsEmpty(export.Group.Item.Common.SelectChar))
      {
        ++local.Error.Count;
      }

      export.Group.Next();
    }

    // ----------------
    // If multiple selects done escape out and display error message.
    // ----------------
    if (local.Error.Count > 0)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Common.SelectChar) != 'S' && !
          IsEmpty(export.Group.Item.Common.SelectChar))
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
    }

    if (local.Count.Count > 1)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
        {
          var field = GetField(export.Group.Item.Common, "selectChar");

          field.Error = true;
        }
      }

      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      // ----------------------------------
      // End  Of change TC # 52
      // ----------------------------------
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ----------------
    // If multiple selects done escape out and display error message.
    // ----------------
    if (Equal(global.Command, "RETCDVL") || Equal
      (global.Command, "RETEXAG") || Equal(global.Command, "EXAG") || Equal
      (global.Command, "EXAL"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "EXAL":
        ExitState = "ZD_ACO_NI000_SEARCH_CRITERIA_REQ";

        break;
      case "EXAG":
        ExitState = "OE0141_EXAG_NOT_AVAILABLE";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        export.Prompt.Text1 = "";

        if (IsEmpty(export.SortBy.SelectChar))
        {
          export.SortBy.SelectChar = "N";
        }

        if (IsEmpty(export.StartingExternalAgency.TypeCode))
        {
          export.AgencyTypeDesc.Description =
            Spaces(CodeValue.Description_MaxLength);
        }
        else
        {
          local.Code.CodeName = "AGENCY TYPE";
          local.CodeValue.Cdvalue = export.StartingExternalAgency.TypeCode ?? Spaces
            (10);
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
          }
          else
          {
            export.AgencyTypeDesc.Description =
              Spaces(CodeValue.Description_MaxLength);

            var field1 = GetField(export.StartingExternalAgency, "typeCode");

            field1.Error = true;

            ExitState = "INVALID_TYPE_CODE";

            return;
          }
        }

        if (AsChar(export.SortBy.SelectChar) == 'N' || AsChar
          (export.SortBy.SelectChar) == 'A' || AsChar
          (export.SortBy.SelectChar) == 'C' || AsChar
          (export.SortBy.SelectChar) == 'T')
        {
        }
        else
        {
          var field1 = GetField(export.SortBy, "selectChar");

          field1.Error = true;

          ExitState = "INVALID_SORT_CHR_MUSTBE_N_T_A_C";

          return;
        }

        if (AsChar(export.SortBy.SelectChar) == 'N')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadExternalAgencyExternalAgencyAddress2())
          {
            if (IsEmpty(entities.ExistingExternalAgency.TypeCode))
            {
            }
            else
            {
              if (!IsEmpty(export.StartingExternalAgency.TypeCode))
              {
                if (Equal(entities.ExistingExternalAgency.TypeCode,
                  import.StartingExternalAgency.TypeCode))
                {
                }
                else
                {
                  export.Group.Next();

                  continue;
                }
              }

              if (entities.ExistingExternalAgency.Identifier == local
                .PrevExternalAgency.Identifier)
              {
                export.Group.Next();

                continue;
              }
              else
              {
                local.PrevExternalAgency.Identifier =
                  entities.ExistingExternalAgency.Identifier;
              }

              export.Group.Update.AgencyExternalAgency.Assign(
                entities.ExistingExternalAgency);
              MoveExternalAgencyAddress(entities.ExistingExternalAgencyAddress,
                export.Group.Update.AgencyExternalAgencyAddress);
            }

            export.Group.Next();
          }
        }

        if (AsChar(export.SortBy.SelectChar) == 'T')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadExternalAgencyExternalAgencyAddress4())
          {
            if (IsEmpty(entities.ExistingExternalAgency.TypeCode))
            {
            }
            else
            {
              if (entities.ExistingExternalAgency.Identifier == local
                .PrevExternalAgency.Identifier)
              {
                export.Group.Next();

                continue;
              }
              else
              {
                local.PrevExternalAgency.Identifier =
                  entities.ExistingExternalAgency.Identifier;
              }

              export.Group.Update.AgencyExternalAgency.Assign(
                entities.ExistingExternalAgency);
              MoveExternalAgencyAddress(entities.ExistingExternalAgencyAddress,
                export.Group.Update.AgencyExternalAgencyAddress);
            }

            export.Group.Next();
          }
        }

        if (AsChar(export.SortBy.SelectChar) == 'A')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadExternalAgencyExternalAgencyAddress3())
          {
            if (IsEmpty(entities.ExistingExternalAgency.TypeCode))
            {
            }
            else
            {
              if (!IsEmpty(export.StartingExternalAgency.TypeCode))
              {
                if (Equal(entities.ExistingExternalAgency.TypeCode,
                  import.StartingExternalAgency.TypeCode))
                {
                }
                else
                {
                  export.Group.Next();

                  continue;
                }
              }

              if (entities.ExistingExternalAgency.Identifier == local
                .PrevExternalAgency.Identifier)
              {
                export.Group.Next();

                continue;
              }
              else
              {
                local.PrevExternalAgency.Identifier =
                  entities.ExistingExternalAgency.Identifier;
              }

              export.Group.Update.AgencyExternalAgency.Assign(
                entities.ExistingExternalAgency);
              MoveExternalAgencyAddress(entities.ExistingExternalAgencyAddress,
                export.Group.Update.AgencyExternalAgencyAddress);
            }

            export.Group.Next();
          }
        }

        if (AsChar(export.SortBy.SelectChar) == 'C')
        {
          export.Group.Index = 0;
          export.Group.Clear();

          foreach(var item in ReadExternalAgencyExternalAgencyAddress1())
          {
            if (IsEmpty(entities.ExistingExternalAgency.TypeCode))
            {
            }
            else
            {
              if (!IsEmpty(export.StartingExternalAgency.TypeCode))
              {
                if (Equal(entities.ExistingExternalAgency.TypeCode,
                  import.StartingExternalAgency.TypeCode))
                {
                }
                else
                {
                  export.Group.Next();

                  continue;
                }
              }

              if (entities.ExistingExternalAgency.Identifier == local
                .PrevExternalAgency.Identifier)
              {
                export.Group.Next();

                continue;
              }
              else
              {
                local.PrevExternalAgency.Identifier =
                  entities.ExistingExternalAgency.Identifier;
              }

              export.Group.Update.AgencyExternalAgency.Assign(
                entities.ExistingExternalAgency);
              MoveExternalAgencyAddress(entities.ExistingExternalAgencyAddress,
                export.Group.Update.AgencyExternalAgencyAddress);
            }

            export.Group.Next();
          }
        }

        if (export.Group.IsFull)
        {
          ExitState = "OE0000_LIST_FULL_PARTIAL_DATA_RT";
        }
        else if (export.Group.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "RETCDVL":
        if (IsEmpty(export.Prompt.Text1))
        {
          // --------------------------------------------------------------
          // Beginning of Change
          // 4.14.100 TC # 22
          // --------------------------------------------------------------
          var field1 = GetField(export.StartingExternalAgency, "typeCode");

          field1.Protected = false;
          field1.Focused = true;

          // --------------------------------------------------------------
          // End of Change
          // 4.14.100 TC # 20
          // --------------------------------------------------------------
        }
        else
        {
          export.Prompt.Text1 = "";
          export.StartingExternalAgency.TypeCode =
            import.HiddenFromList.Cdvalue;
          export.AgencyTypeDesc.Description = import.HiddenFromList.Description;

          // --------------------------------------------------------------
          // Beginning of Change
          // 4.14.100 TC # 22
          // --------------------------------------------------------------
          var field1 = GetField(export.SortBy, "selectChar");

          field1.Protected = false;
          field1.Focused = true;

          // --------------------------------------------------------------
          // End of Change
          // 4.14.100 TC # 20
          // --------------------------------------------------------------
        }

        break;
      case "LIST":
        // --------------------------------------------------------------
        // Beginning of Change
        // 4.14.100 TC # 20
        // --------------------------------------------------------------
        if (!IsEmpty(export.Prompt.Text1) && AsChar(export.Prompt.Text1) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.Prompt, "text1");

          field1.Error = true;

          return;
        }

        // --------------------------------------------------------------
        // End of Change
        // 4.14.100 TC # 20
        // --------------------------------------------------------------
        if (AsChar(export.Prompt.Text1) == 'S')
        {
          export.HiddenToCodeTableList.CodeName = "AGENCY TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        // --------------------------------------------------------------
        // Beginning of Change
        // 4.14.100 TC # 19
        // --------------------------------------------------------------
        var field = GetField(export.Prompt, "text1");

        field.Error = true;

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        // --------------------------------------------------------------
        // End of Change
        // 4.14.100 TC # 19
        // --------------------------------------------------------------
        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveExternalAgencyAddress(ExternalAgencyAddress source,
    ExternalAgencyAddress target)
  {
    target.Type1 = source.Type1;
    target.City = source.City;
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

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    export.AgencyTypeDesc.Description = useExport.CodeValue.Description;
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
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadExternalAgencyExternalAgencyAddress1()
  {
    return ReadEach("ReadExternalAgencyExternalAgencyAddress1",
      (db, command) =>
      {
        db.SetString(command, "name", import.StartingExternalAgency.Name);
        db.
          SetString(command, "city", import.StartingExternalAgencyAddress.City);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingExternalAgency.Identifier = db.GetInt32(reader, 0);
        entities.ExistingExternalAgencyAddress.ExaId = db.GetInt32(reader, 0);
        entities.ExistingExternalAgency.TypeCode =
          db.GetNullableString(reader, 1);
        entities.ExistingExternalAgency.Name = db.GetString(reader, 2);
        entities.ExistingExternalAgency.PhoneAreaCode =
          db.GetNullableInt32(reader, 3);
        entities.ExistingExternalAgency.Phone = db.GetNullableInt32(reader, 4);
        entities.ExistingExternalAgencyAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingExternalAgencyAddress.City = db.GetString(reader, 6);
        entities.ExistingExternalAgency.Populated = true;
        entities.ExistingExternalAgencyAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadExternalAgencyExternalAgencyAddress2()
  {
    return ReadEach("ReadExternalAgencyExternalAgencyAddress2",
      (db, command) =>
      {
        db.SetString(command, "name", import.StartingExternalAgency.Name);
        db.
          SetString(command, "city", import.StartingExternalAgencyAddress.City);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingExternalAgency.Identifier = db.GetInt32(reader, 0);
        entities.ExistingExternalAgencyAddress.ExaId = db.GetInt32(reader, 0);
        entities.ExistingExternalAgency.TypeCode =
          db.GetNullableString(reader, 1);
        entities.ExistingExternalAgency.Name = db.GetString(reader, 2);
        entities.ExistingExternalAgency.PhoneAreaCode =
          db.GetNullableInt32(reader, 3);
        entities.ExistingExternalAgency.Phone = db.GetNullableInt32(reader, 4);
        entities.ExistingExternalAgencyAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingExternalAgencyAddress.City = db.GetString(reader, 6);
        entities.ExistingExternalAgency.Populated = true;
        entities.ExistingExternalAgencyAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadExternalAgencyExternalAgencyAddress3()
  {
    return ReadEach("ReadExternalAgencyExternalAgencyAddress3",
      (db, command) =>
      {
        db.SetString(command, "name", import.StartingExternalAgency.Name);
        db.
          SetString(command, "city", import.StartingExternalAgencyAddress.City);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingExternalAgency.Identifier = db.GetInt32(reader, 0);
        entities.ExistingExternalAgencyAddress.ExaId = db.GetInt32(reader, 0);
        entities.ExistingExternalAgency.TypeCode =
          db.GetNullableString(reader, 1);
        entities.ExistingExternalAgency.Name = db.GetString(reader, 2);
        entities.ExistingExternalAgency.PhoneAreaCode =
          db.GetNullableInt32(reader, 3);
        entities.ExistingExternalAgency.Phone = db.GetNullableInt32(reader, 4);
        entities.ExistingExternalAgencyAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingExternalAgencyAddress.City = db.GetString(reader, 6);
        entities.ExistingExternalAgency.Populated = true;
        entities.ExistingExternalAgencyAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadExternalAgencyExternalAgencyAddress4()
  {
    return ReadEach("ReadExternalAgencyExternalAgencyAddress4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "typeCode", export.StartingExternalAgency.TypeCode ?? "");
        db.SetString(command, "name", import.StartingExternalAgency.Name);
        db.
          SetString(command, "city", import.StartingExternalAgencyAddress.City);
          
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingExternalAgency.Identifier = db.GetInt32(reader, 0);
        entities.ExistingExternalAgencyAddress.ExaId = db.GetInt32(reader, 0);
        entities.ExistingExternalAgency.TypeCode =
          db.GetNullableString(reader, 1);
        entities.ExistingExternalAgency.Name = db.GetString(reader, 2);
        entities.ExistingExternalAgency.PhoneAreaCode =
          db.GetNullableInt32(reader, 3);
        entities.ExistingExternalAgency.Phone = db.GetNullableInt32(reader, 4);
        entities.ExistingExternalAgencyAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingExternalAgencyAddress.City = db.GetString(reader, 6);
        entities.ExistingExternalAgency.Populated = true;
        entities.ExistingExternalAgencyAddress.Populated = true;

        return true;
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
      /// A value of AgencyExternalAgency.
      /// </summary>
      [JsonPropertyName("agencyExternalAgency")]
      public ExternalAgency AgencyExternalAgency
      {
        get => agencyExternalAgency ??= new();
        set => agencyExternalAgency = value;
      }

      /// <summary>
      /// A value of AgencyExternalAgencyAddress.
      /// </summary>
      [JsonPropertyName("agencyExternalAgencyAddress")]
      public ExternalAgencyAddress AgencyExternalAgencyAddress
      {
        get => agencyExternalAgencyAddress ??= new();
        set => agencyExternalAgencyAddress = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ExternalAgency agencyExternalAgency;
      private ExternalAgencyAddress agencyExternalAgencyAddress;
      private Common common;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public TextWorkArea Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of HiddenExternalAgency.
    /// </summary>
    [JsonPropertyName("hiddenExternalAgency")]
    public ExternalAgency HiddenExternalAgency
    {
      get => hiddenExternalAgency ??= new();
      set => hiddenExternalAgency = value;
    }

    /// <summary>
    /// A value of StartingExternalAgency.
    /// </summary>
    [JsonPropertyName("startingExternalAgency")]
    public ExternalAgency StartingExternalAgency
    {
      get => startingExternalAgency ??= new();
      set => startingExternalAgency = value;
    }

    /// <summary>
    /// A value of StartingExternalAgencyAddress.
    /// </summary>
    [JsonPropertyName("startingExternalAgencyAddress")]
    public ExternalAgencyAddress StartingExternalAgencyAddress
    {
      get => startingExternalAgencyAddress ??= new();
      set => startingExternalAgencyAddress = value;
    }

    /// <summary>
    /// A value of AgencyTypeDesc.
    /// </summary>
    [JsonPropertyName("agencyTypeDesc")]
    public CodeValue AgencyTypeDesc
    {
      get => agencyTypeDesc ??= new();
      set => agencyTypeDesc = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
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
    /// A value of PromptAgencyType.
    /// </summary>
    [JsonPropertyName("promptAgencyType")]
    public Common PromptAgencyType
    {
      get => promptAgencyType ??= new();
      set => promptAgencyType = value;
    }

    /// <summary>
    /// A value of HiddenFromList.
    /// </summary>
    [JsonPropertyName("hiddenFromList")]
    public CodeValue HiddenFromList
    {
      get => hiddenFromList ??= new();
      set => hiddenFromList = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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

    private TextWorkArea prompt;
    private ExternalAgency hiddenExternalAgency;
    private ExternalAgency startingExternalAgency;
    private ExternalAgencyAddress startingExternalAgencyAddress;
    private CodeValue agencyTypeDesc;
    private Common sortBy;
    private Array<GroupGroup> group;
    private Common promptAgencyType;
    private CodeValue hiddenFromList;
    private NextTranInfo nextTranInfo;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
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
      /// A value of AgencyExternalAgency.
      /// </summary>
      [JsonPropertyName("agencyExternalAgency")]
      public ExternalAgency AgencyExternalAgency
      {
        get => agencyExternalAgency ??= new();
        set => agencyExternalAgency = value;
      }

      /// <summary>
      /// A value of AgencyExternalAgencyAddress.
      /// </summary>
      [JsonPropertyName("agencyExternalAgencyAddress")]
      public ExternalAgencyAddress AgencyExternalAgencyAddress
      {
        get => agencyExternalAgencyAddress ??= new();
        set => agencyExternalAgencyAddress = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private ExternalAgency agencyExternalAgency;
      private ExternalAgencyAddress agencyExternalAgencyAddress;
      private Common common;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public TextWorkArea Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public ExternalAgency Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of StartingExternalAgency.
    /// </summary>
    [JsonPropertyName("startingExternalAgency")]
    public ExternalAgency StartingExternalAgency
    {
      get => startingExternalAgency ??= new();
      set => startingExternalAgency = value;
    }

    /// <summary>
    /// A value of StartingExternalAgencyAddress.
    /// </summary>
    [JsonPropertyName("startingExternalAgencyAddress")]
    public ExternalAgencyAddress StartingExternalAgencyAddress
    {
      get => startingExternalAgencyAddress ??= new();
      set => startingExternalAgencyAddress = value;
    }

    /// <summary>
    /// A value of SortBy.
    /// </summary>
    [JsonPropertyName("sortBy")]
    public Common SortBy
    {
      get => sortBy ??= new();
      set => sortBy = value;
    }

    /// <summary>
    /// A value of HiddenListCancel.
    /// </summary>
    [JsonPropertyName("hiddenListCancel")]
    public Common HiddenListCancel
    {
      get => hiddenListCancel ??= new();
      set => hiddenListCancel = value;
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
    /// A value of PromptAgencyType.
    /// </summary>
    [JsonPropertyName("promptAgencyType")]
    public Common PromptAgencyType
    {
      get => promptAgencyType ??= new();
      set => promptAgencyType = value;
    }

    /// <summary>
    /// A value of HiddenToCodeTableList.
    /// </summary>
    [JsonPropertyName("hiddenToCodeTableList")]
    public Code HiddenToCodeTableList
    {
      get => hiddenToCodeTableList ??= new();
      set => hiddenToCodeTableList = value;
    }

    /// <summary>
    /// A value of AgencyTypeDesc.
    /// </summary>
    [JsonPropertyName("agencyTypeDesc")]
    public CodeValue AgencyTypeDesc
    {
      get => agencyTypeDesc ??= new();
      set => agencyTypeDesc = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private TextWorkArea prompt;
    private ExternalAgency selected;
    private ExternalAgency startingExternalAgency;
    private ExternalAgencyAddress startingExternalAgencyAddress;
    private Common sortBy;
    private Common hiddenListCancel;
    private Array<GroupGroup> group;
    private Common promptAgencyType;
    private Code hiddenToCodeTableList;
    private CodeValue agencyTypeDesc;
    private NextTranInfo nextTranInfo;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of PrevExternalAgency.
    /// </summary>
    [JsonPropertyName("prevExternalAgency")]
    public ExternalAgency PrevExternalAgency
    {
      get => prevExternalAgency ??= new();
      set => prevExternalAgency = value;
    }

    /// <summary>
    /// A value of PrevOffice.
    /// </summary>
    [JsonPropertyName("prevOffice")]
    public Office PrevOffice
    {
      get => prevOffice ??= new();
      set => prevOffice = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    private Common error;
    private ExternalAgency prevExternalAgency;
    private Office prevOffice;
    private Common count;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common returnCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingExternalAgency.
    /// </summary>
    [JsonPropertyName("existingExternalAgency")]
    public ExternalAgency ExistingExternalAgency
    {
      get => existingExternalAgency ??= new();
      set => existingExternalAgency = value;
    }

    /// <summary>
    /// A value of ExistingExternalAgencyAddress.
    /// </summary>
    [JsonPropertyName("existingExternalAgencyAddress")]
    public ExternalAgencyAddress ExistingExternalAgencyAddress
    {
      get => existingExternalAgencyAddress ??= new();
      set => existingExternalAgencyAddress = value;
    }

    private ExternalAgency existingExternalAgency;
    private ExternalAgencyAddress existingExternalAgencyAddress;
  }
#endregion
}
