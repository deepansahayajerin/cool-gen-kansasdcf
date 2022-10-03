// Program: LE_DISPLAY_IWO_GARNISHMENT_LIEN, ID: 372028999, model: 746.
// Short name: SWE00766
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DISPLAY_IWO_GARNISHMENT_LIEN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action diagram will display information about an Income Withholding 
/// Order (IWO), Garnishment, or Lien for a specific Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeDisplayIwoGarnishmentLien: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISPLAY_IWO_GARNISHMENT_LIEN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDisplayIwoGarnishmentLien(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDisplayIwoGarnishmentLien.
  /// </summary>
  public LeDisplayIwoGarnishmentLien(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/29/95	Dave Allen			Initial Code
    // 08/10/01 	G Vandy		PR 124953	Read name from Employer, if available, 
    // instead
    // 						of from Income Source.
    // -----------------------------------------------------------------------------------------------
    export.LegalAction.Assign(entities.LegalAction);
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    export.IwglType.Text1 = import.SearchIwglType.Text1;

    if (ReadLegalAction())
    {
      export.LegalAction.Assign(entities.LegalAction);
      local.Code.CodeName = "ACTION TAKEN";
      local.CodeValue.Cdvalue = export.LegalAction.ActionTaken;
      UseCabGetCodeValueDescription();

      if (!IsEmpty(local.CodeValue.Description))
      {
        export.LegalAction.ActionTaken = local.CodeValue.Description;
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    if (ReadCsePerson())
    {
      // ------------------------------------------------------------
      // Read Person ADABASE table using External Action Block.
      // ------------------------------------------------------------
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.IwglRecFound.Flag = "N";

    switch(TrimEnd(import.UserAction.Command))
    {
      case "DISPLAY":
        if (import.LegalActionIncomeSource.Identifier > 0)
        {
          if (AsChar(import.SearchIwglType.Text1) == 'I' || AsChar
            (import.SearchIwglType.Text1) == 'G' && AsChar
            (import.SearchForGarnishment.WageOrNonWage) == 'W')
          {
            if (ReadLegalActionIncomeSourceIncomeSource1())
            {
              local.IwglRecFound.Flag = "Y";

              if (!IsEmpty(entities.LegalActionIncomeSource.WithholdingType))
              {
                export.IwglType.Text1 = "I";
              }
              else
              {
                export.IwglType.Text1 = "G";
              }

              MoveIncomeSource(entities.IncomeSource, export.IncomeSource);
              export.LegalActionIncomeSource.Assign(
                entities.LegalActionIncomeSource);
            }
          }
          else if (AsChar(import.SearchIwglType.Text1) == 'L' || AsChar
            (import.SearchIwglType.Text1) == 'G' && AsChar
            (import.SearchForGarnishment.WageOrNonWage) == 'N')
          {
            if (ReadLegalActionPersonResourceCsePersonResource1())
            {
              local.IwglRecFound.Flag = "Y";
              export.LegalActionPersonResource.Assign(
                entities.LegalActionPersonResource);
              MoveCsePersonResource(entities.CsePersonResource,
                export.CsePersonResource);
              export.LegalActionIncomeSource.EffectiveDate =
                entities.LegalActionPersonResource.EffectiveDate;
              export.LegalActionIncomeSource.EndDate =
                entities.LegalActionPersonResource.EndDate;
              export.LegalActionIncomeSource.CreatedTstamp =
                entities.LegalActionPersonResource.CreatedTstamp;

              if (!IsEmpty(entities.LegalActionPersonResource.LienType))
              {
                export.IwglType.Text1 = "L";
              }
              else
              {
                export.IwglType.Text1 = "G";
                export.LegalActionIncomeSource.WageOrNonWage = "N";
              }
            }
          }
        }
        else if (AsChar(import.SearchIwglType.Text1) == 'I' || AsChar
          (import.SearchIwglType.Text1) == 'G' && AsChar
          (import.SearchForGarnishment.WageOrNonWage) == 'W')
        {
          if (ReadLegalActionIncomeSourceIncomeSource2())
          {
            local.IwglRecFound.Flag = "Y";

            if (!IsEmpty(entities.LegalActionIncomeSource.WithholdingType))
            {
              export.IwglType.Text1 = "I";
            }
            else
            {
              export.IwglType.Text1 = "G";
            }

            MoveIncomeSource(entities.IncomeSource, export.IncomeSource);
            export.LegalActionIncomeSource.Assign(
              entities.LegalActionIncomeSource);
          }
        }
        else if (AsChar(import.SearchIwglType.Text1) == 'L' || AsChar
          (import.SearchIwglType.Text1) == 'G' && AsChar
          (import.SearchForGarnishment.WageOrNonWage) == 'N')
        {
          if (ReadLegalActionPersonResourceCsePersonResource2())
          {
            local.IwglRecFound.Flag = "Y";
            export.LegalActionPersonResource.Assign(
              entities.LegalActionPersonResource);
            MoveCsePersonResource(entities.CsePersonResource,
              export.CsePersonResource);
            export.LegalActionIncomeSource.EffectiveDate =
              entities.LegalActionPersonResource.EffectiveDate;
            export.LegalActionIncomeSource.EndDate =
              entities.LegalActionPersonResource.EndDate;
            export.LegalActionIncomeSource.CreatedTstamp =
              entities.LegalActionPersonResource.CreatedTstamp;

            if (!IsEmpty(entities.LegalActionPersonResource.LienType))
            {
              export.IwglType.Text1 = "L";
            }
            else
            {
              export.IwglType.Text1 = "G";
              export.LegalActionIncomeSource.WageOrNonWage = "N";
            }
          }
        }

        if (AsChar(local.IwglRecFound.Flag) == 'N')
        {
          ExitState = "LE0000_IWGL_NF";

          return;
        }

        break;
      case "PREV":
        if (AsChar(import.SearchIwglType.Text1) == 'I' || AsChar
          (import.SearchIwglType.Text1) == 'G' && AsChar
          (import.SearchForGarnishment.WageOrNonWage) == 'W')
        {
          foreach(var item in ReadLegalActionIncomeSourceIncomeSource4())
          {
            if (Lt(entities.LegalActionIncomeSource.CreatedTstamp,
              import.LegalActionIncomeSource.CreatedTstamp))
            {
              local.IwglRecFound.Flag = "Y";

              if (!IsEmpty(entities.LegalActionIncomeSource.WithholdingType))
              {
                export.IwglType.Text1 = "I";
              }
              else
              {
                export.IwglType.Text1 = "G";
              }

              MoveIncomeSource(entities.IncomeSource, export.IncomeSource);
              export.LegalActionIncomeSource.Assign(
                entities.LegalActionIncomeSource);

              goto Test;
            }
          }
        }
        else if (AsChar(import.SearchIwglType.Text1) == 'L' || AsChar
          (import.SearchIwglType.Text1) == 'G' && AsChar
          (import.SearchForGarnishment.WageOrNonWage) == 'N')
        {
          foreach(var item in ReadLegalActionPersonResourceCsePersonResource4())
          {
            if (Lt(entities.LegalActionPersonResource.CreatedTstamp,
              import.LegalActionIncomeSource.CreatedTstamp))
            {
              export.LegalActionPersonResource.Assign(
                entities.LegalActionPersonResource);
              MoveCsePersonResource(entities.CsePersonResource,
                export.CsePersonResource);
              export.LegalActionIncomeSource.EffectiveDate =
                entities.LegalActionPersonResource.EffectiveDate;
              export.LegalActionIncomeSource.EndDate =
                entities.LegalActionPersonResource.EndDate;
              export.LegalActionIncomeSource.CreatedTstamp =
                export.LegalActionPersonResource.CreatedTstamp;

              if (!IsEmpty(entities.LegalActionPersonResource.LienType))
              {
                export.IwglType.Text1 = "L";
              }
              else
              {
                export.IwglType.Text1 = "G";
                export.LegalActionIncomeSource.WageOrNonWage = "N";
              }

              local.IwglRecFound.Flag = "Y";

              goto Test;
            }
          }
        }

        if (AsChar(local.IwglRecFound.Flag) == 'N')
        {
          ExitState = "LE0000_NO_MORE_IWGL_RECORD";

          return;
        }

        ExitState = "LE0000_NO_MORE_IWGL_RECORD";

        break;
      case "NEXT":
        if (AsChar(import.SearchIwglType.Text1) == 'I' || AsChar
          (import.SearchIwglType.Text1) == 'G' && AsChar
          (import.SearchForGarnishment.WageOrNonWage) == 'W')
        {
          foreach(var item in ReadLegalActionIncomeSourceIncomeSource3())
          {
            if (Lt(import.LegalActionIncomeSource.CreatedTstamp,
              entities.LegalActionIncomeSource.CreatedTstamp))
            {
              local.IwglRecFound.Flag = "Y";
              MoveIncomeSource(entities.IncomeSource, export.IncomeSource);
              export.LegalActionIncomeSource.Assign(
                entities.LegalActionIncomeSource);

              if (!IsEmpty(entities.LegalActionIncomeSource.WithholdingType))
              {
                export.IwglType.Text1 = "I";
              }
              else
              {
                export.IwglType.Text1 = "G";
              }

              goto Test;
            }
          }
        }
        else if (AsChar(import.SearchIwglType.Text1) == 'L' || AsChar
          (import.SearchIwglType.Text1) == 'G' && AsChar
          (import.SearchForGarnishment.WageOrNonWage) == 'N')
        {
          foreach(var item in ReadLegalActionPersonResourceCsePersonResource3())
          {
            if (Lt(import.LegalActionIncomeSource.CreatedTstamp,
              entities.LegalActionPersonResource.CreatedTstamp))
            {
              export.LegalActionPersonResource.Assign(
                entities.LegalActionPersonResource);
              MoveCsePersonResource(entities.CsePersonResource,
                export.CsePersonResource);
              export.LegalActionIncomeSource.EffectiveDate =
                entities.LegalActionPersonResource.EffectiveDate;
              export.LegalActionIncomeSource.EndDate =
                entities.LegalActionPersonResource.EndDate;
              export.LegalActionIncomeSource.CreatedTstamp =
                export.LegalActionPersonResource.CreatedTstamp;

              if (!IsEmpty(entities.LegalActionPersonResource.LienType))
              {
                export.IwglType.Text1 = "L";
              }
              else
              {
                export.IwglType.Text1 = "G";
                export.LegalActionIncomeSource.WageOrNonWage = "N";
              }

              local.IwglRecFound.Flag = "Y";

              goto Test;
            }
          }
        }

        if (AsChar(local.IwglRecFound.Flag) == 'N')
        {
          ExitState = "LE0000_NO_MORE_IWGL_RECORD";

          return;
        }

        break;
      default:
        break;
    }

Test:

    if (entities.IncomeSource.Populated)
    {
      // 08/10/01  G Vandy - PR 124953 - Read name from Employer, if available, 
      // instead of from Income Source.
      if (ReadEmployer())
      {
        export.IncomeSource.Name = entities.Employer.Name;
      }
      else
      {
        // -- Continue.  Income source is not always associated to an employer.
      }
    }

    // ---------------------------------------------
    // Now set up scrolling attributes plus and minus flags
    // ---------------------------------------------
    if (AsChar(import.SearchIwglType.Text1) == 'I' || AsChar
      (import.SearchIwglType.Text1) == 'G' && AsChar
      (import.SearchForGarnishment.WageOrNonWage) == 'W')
    {
      foreach(var item in ReadLegalActionIncomeSourceIncomeSource4())
      {
        if (Lt(export.LegalActionIncomeSource.CreatedTstamp,
          entities.LegalActionIncomeSource.CreatedTstamp))
        {
          export.ScrollingAttributes.PlusFlag = "+";
        }

        if (Lt(entities.LegalActionIncomeSource.CreatedTstamp,
          export.LegalActionIncomeSource.CreatedTstamp))
        {
          export.ScrollingAttributes.MinusFlag = "-";
        }
      }
    }
    else if (AsChar(import.SearchIwglType.Text1) == 'L' || AsChar
      (import.SearchIwglType.Text1) == 'G' && AsChar
      (import.SearchForGarnishment.WageOrNonWage) == 'N')
    {
      foreach(var item in ReadLegalActionPersonResourceCsePersonResource4())
      {
        if (Lt(export.LegalActionIncomeSource.CreatedTstamp,
          entities.LegalActionPersonResource.CreatedTstamp))
        {
          export.ScrollingAttributes.PlusFlag = "+";
        }

        if (Lt(entities.LegalActionPersonResource.CreatedTstamp,
          export.LegalActionIncomeSource.CreatedTstamp))
        {
          export.ScrollingAttributes.PlusFlag = "-";
        }
      }
    }
  }

  private static void MoveCsePersonResource(CsePersonResource source,
    CsePersonResource target)
  {
    target.ResourceNo = source.ResourceNo;
    target.ResourceDescription = source.ResourceDescription;
  }

  private static void MoveIncomeSource(IncomeSource source, IncomeSource target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.CodeValue.Assign(useExport.CodeValue);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadEmployer()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.Employer.Populated = true;
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
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionIncomeSourceIncomeSource1()
  {
    entities.IncomeSource.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSourceIncomeSource1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(
          command, "identifier", import.LegalActionIncomeSource.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 5);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 7);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 8);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 9);
        entities.IncomeSource.Name = db.GetNullableString(reader, 10);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 11);
        entities.IncomeSource.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
      });
  }

  private bool ReadLegalActionIncomeSourceIncomeSource2()
  {
    entities.IncomeSource.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSourceIncomeSource2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 5);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 7);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 8);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 9);
        entities.IncomeSource.Name = db.GetNullableString(reader, 10);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 11);
        entities.IncomeSource.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionIncomeSourceIncomeSource3()
  {
    entities.IncomeSource.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return ReadEach("ReadLegalActionIncomeSourceIncomeSource3",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDateTime(
          command, "createdTstamp",
          import.LegalActionIncomeSource.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 5);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 7);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 8);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 9);
        entities.IncomeSource.Name = db.GetNullableString(reader, 10);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 11);
        entities.IncomeSource.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionIncomeSourceIncomeSource4()
  {
    entities.IncomeSource.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return ReadEach("ReadLegalActionIncomeSourceIncomeSource4",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 0);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 2);
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 2);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionIncomeSource.CreatedTstamp =
          db.GetDateTime(reader, 4);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 5);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionIncomeSource.WageOrNonWage =
          db.GetNullableString(reader, 7);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 8);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 9);
        entities.IncomeSource.Name = db.GetNullableString(reader, 10);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 11);
        entities.IncomeSource.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;

        return true;
      });
  }

  private bool ReadLegalActionPersonResourceCsePersonResource1()
  {
    entities.CsePersonResource.Populated = false;
    entities.LegalActionPersonResource.Populated = false;

    return Read("ReadLegalActionPersonResourceCsePersonResource1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(
          command, "identifier", import.LegalActionIncomeSource.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 0);
        entities.CsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionPersonResource.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 7);
        entities.CsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 8);
        entities.CsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 9);
        entities.CsePersonResource.Populated = true;
        entities.LegalActionPersonResource.Populated = true;
      });
  }

  private bool ReadLegalActionPersonResourceCsePersonResource2()
  {
    entities.CsePersonResource.Populated = false;
    entities.LegalActionPersonResource.Populated = false;

    return Read("ReadLegalActionPersonResourceCsePersonResource2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 0);
        entities.CsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionPersonResource.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 7);
        entities.CsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 8);
        entities.CsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 9);
        entities.CsePersonResource.Populated = true;
        entities.LegalActionPersonResource.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonResourceCsePersonResource3()
  {
    entities.CsePersonResource.Populated = false;
    entities.LegalActionPersonResource.Populated = false;

    return ReadEach("ReadLegalActionPersonResourceCsePersonResource3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 0);
        entities.CsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionPersonResource.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 7);
        entities.CsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 8);
        entities.CsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 9);
        entities.CsePersonResource.Populated = true;
        entities.LegalActionPersonResource.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionPersonResourceCsePersonResource4()
  {
    entities.CsePersonResource.Populated = false;
    entities.LegalActionPersonResource.Populated = false;

    return ReadEach("ReadLegalActionPersonResourceCsePersonResource4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 0);
        entities.CsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.LegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionPersonResource.CreatedTstamp =
          db.GetDateTime(reader, 6);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 7);
        entities.CsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 8);
        entities.CsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 9);
        entities.CsePersonResource.Populated = true;
        entities.LegalActionPersonResource.Populated = true;

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
    /// <summary>
    /// A value of SearchForGarnishment.
    /// </summary>
    [JsonPropertyName("searchForGarnishment")]
    public LegalActionIncomeSource SearchForGarnishment
    {
      get => searchForGarnishment ??= new();
      set => searchForGarnishment = value;
    }

    /// <summary>
    /// A value of SearchIwglType.
    /// </summary>
    [JsonPropertyName("searchIwglType")]
    public WorkArea SearchIwglType
    {
      get => searchIwglType ??= new();
      set => searchIwglType = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    private LegalActionIncomeSource searchForGarnishment;
    private WorkArea searchIwglType;
    private Common userAction;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private IncomeSource incomeSource;
    private LegalActionIncomeSource legalActionIncomeSource;
    private CsePersonResource csePersonResource;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of IwglType.
    /// </summary>
    [JsonPropertyName("iwglType")]
    public WorkArea IwglType
    {
      get => iwglType ??= new();
      set => iwglType = value;
    }

    private ScrollingAttributes scrollingAttributes;
    private LegalAction legalAction;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IncomeSource incomeSource;
    private LegalActionPersonResource legalActionPersonResource;
    private CsePersonResource csePersonResource;
    private WorkArea iwglType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of IwglRecFound.
    /// </summary>
    [JsonPropertyName("iwglRecFound")]
    public Common IwglRecFound
    {
      get => iwglRecFound ??= new();
      set => iwglRecFound = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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

    private CodeValue codeValue;
    private Code code;
    private Common iwglRecFound;
    private DateWorkArea zero;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
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
    /// A value of LegalActionIncomeSource.
    /// </summary>
    [JsonPropertyName("legalActionIncomeSource")]
    public LegalActionIncomeSource LegalActionIncomeSource
    {
      get => legalActionIncomeSource ??= new();
      set => legalActionIncomeSource = value;
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
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of LegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("legalActionPersonResource")]
    public LegalActionPersonResource LegalActionPersonResource
    {
      get => legalActionPersonResource ??= new();
      set => legalActionPersonResource = value;
    }

    private Employer employer;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private LegalActionIncomeSource legalActionIncomeSource;
    private LegalAction legalAction;
    private CsePersonResource csePersonResource;
    private LegalActionPersonResource legalActionPersonResource;
  }
#endregion
}
