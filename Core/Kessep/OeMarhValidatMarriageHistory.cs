// Program: OE_MARH_VALIDAT_MARRIAGE_HISTORY, ID: 371884871, model: 746.
// Short name: SWE00947
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
/// A program: OE_MARH_VALIDAT_MARRIAGE_HISTORY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This BSD action block validates marriage history details.
/// </para>
/// </summary>
[Serializable]
public partial class OeMarhValidatMarriageHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MARH_VALIDAT_MARRIAGE_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMarhValidatMarriageHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMarhValidatMarriageHistory.
  /// </summary>
  public OeMarhValidatMarriageHistory(IContext context, Import import,
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
    // *********************************************
    // Attributes MARRIAGE_COUNTRYand DIVORCE_COUNTRY
    // need to be added to MARRIAGE HISTORY entity. After adding them,
    // include validations for those attributes.
    // *********************************************
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block validates the input for maintenance of CSE Person 
    // Marriage History.
    // PROCESSING:
    // The details are validated in natural order of the screen fields (Left to 
    // Right, Top to Bottom). The errors are returned in a repeating group and
    // highlighted in reverse order by the procedure step.
    // The following views are passed:
    // For CREATE new marriage history details
    // For UPDATE existing and new marriage history details.
    // For DELETE existing marriage history details.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			- R - -
    // 	MARRIAGE_HISTORY	- R - -
    // 	CODE			- R - -
    // 	CODE_VALUE		- R - -
    // DATABASE FILES USED:
    // CREATED BY:		govindaraj.
    // DATE CREATED:		01-04-1995.
    // *********************************************
    // NOTE:
    // ------------------------------------------------------------
    // Date	Developer	Request		# Description
    // 2/4/10	JHuss		CQ# 14066	Modified update validation logic to use more 
    // granular approach.  Apparently,
    // 					another developer had made a similar change to the create validation
    // logic,
    // 					but did not document it.  These changes were made as the previous 
    // logic
    // 					caused ineffcient paths to be taken in DB2 causing 'runaway' 
    // queries.
    // ------------------------------------------------------------
    export.LastErrorEntryNo.Count = 0;
    export.ErrorCodes.Index = -1;
    export.PrimeCsePerson.Number = import.PrimeCsePerson.Number;
    export.SpouseCsePerson.Number = import.NewSpouseCsePerson.Number;
    export.PrimeCsePersonsWorkSet.FormattedName =
      import.PrimeCsePersonsWorkSet.FormattedName;
    export.SpouseCsePersonsWorkSet.FormattedName = import.Spouse.FormattedName;

    if (IsEmpty(import.PrimeCsePerson.Number))
    {
      export.PrimeCsePersonsWorkSet.FormattedName = "";

      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      export.ErrorCodes.Update.EntryErrorCode.Count = 1;

      return;
    }
    else if (ReadCsePerson2())
    {
      export.PrimeCsePerson.Number = entities.ExistingPrime.Number;
      UseCabGetClientDetails1();
    }
    else
    {
      export.PrimeCsePersonsWorkSet.FormattedName = "";

      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      export.ErrorCodes.Update.EntryErrorCode.Count = 1;

      return;
    }

    if (Equal(import.UserAction.Command, "CREATE"))
    {
      // ---------------------------------------------
      // Check if a marriage history rec already exists between the same persons
      //   Recorded for            Spouse as
      //   input CSE Person        another CSE Person
      //   input CSE Person        Contact
      //             OR
      //    another CSE Person     input CSE Person
      // ---------------------------------------------
      if (!IsEmpty(import.NewSpouseCsePerson.Number))
      {
        if (ReadMarriageHistory6())
        {
          // ---------------------------------------------
          // Duplicate marriage history detail.
          // ---------------------------------------------
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 22;

          return;
        }
      }
      else if (ReadMarriageHistory2())
      {
        // ---------------------------------------------
        // Duplicate marriage history detail.
        // ---------------------------------------------
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 22;

        return;
      }

      if (ReadMarriageHistory5())
      {
        // ---------------------------------------------
        // Duplicate marriage history detail.
        // ---------------------------------------------
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 22;

        return;
      }
    }

    if (Equal(import.UserAction.Command, "UPDATE"))
    {
      // 2/4/2010	JHuss	Updated logic to use more granular approach in checking 
      // CSE Persons & Contacts.
      if (!IsEmpty(import.NewSpouseCsePerson.Number))
      {
        if (ReadMarriageHistory3())
        {
          // ---------------------------------------------
          // Duplicate marriage history detail.
          // ---------------------------------------------
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 22;

          return;
        }
      }
      else if (ReadMarriageHistory1())
      {
        // ---------------------------------------------
        // Duplicate marriage history detail.
        // ---------------------------------------------
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 22;

        return;
      }

      if (ReadMarriageHistory4())
      {
        // ---------------------------------------------
        // Duplicate marriage history detail.
        // ---------------------------------------------
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 22;

        return;
      }
    }

    if (Equal(import.UserAction.Command, "CREATE") || Equal
      (import.UserAction.Command, "UPDATE"))
    {
      // New Marriage history must be supplied.
      if (IsEmpty(import.NewSpouseCsePerson.Number) && IsEmpty
        (import.NewSpouseContact.NameLast) && IsEmpty
        (import.NewSpouseContact.NameFirst) && IsEmpty
        (import.NewSpouseContact.MiddleInitial))
      {
        // Neither a CSE Person nor a Contact specified as spouse.
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 2;

        return;
      }

      if (!IsEmpty(import.NewSpouseCsePerson.Number) && (
        !IsEmpty(import.NewSpouseContact.NameLast) || !
        IsEmpty(import.NewSpouseContact.NameFirst) || !
        IsEmpty(import.NewSpouseContact.MiddleInitial)))
      {
        // Specified a CSE Person AND a Contact as spouse. Only one of them must
        // be supplied.
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 3;

        // ---------------------------------------------
        // This ESCAPE was added to avoid fatal error in attempting to read 
        // CONTACT later below.
        return;
      }

      if (!IsEmpty(import.NewSpouseCsePerson.Number))
      {
        if (ReadCsePerson1())
        {
          export.SpouseCsePerson.Number =
            entities.ExistingNewSpouseCsePerson.Number;
          UseCabGetClientDetails2();
        }
        else
        {
          export.SpouseCsePersonsWorkSet.FormattedName = "";

          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 4;

          return;
        }
      }
      else
      {
        // ---------------------------------------------
        // Then spouse is a contact
        // ---------------------------------------------
      }
    }

    if (Equal(import.UserAction.Command, "UPDATE") || Equal
      (import.UserAction.Command, "DELETE"))
    {
      // Current marriage history details must be supplied.
      if (!ReadMarriageHistory7())
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 8;

        return;
      }
    }

    // For Create and Update functions, validate marriage and divorce county, 
    // state and country.
    if (Equal(import.UserAction.Command, "CREATE") || Equal
      (import.UserAction.Command, "UPDATE"))
    {
      if (!IsEmpty(import.NewSpouseContact.NameFirst) || !
        IsEmpty(import.NewSpouseContact.NameFirst) || !
        IsEmpty(import.NewSpouseContact.MiddleInitial))
      {
        if (IsEmpty(import.NewSpouseContact.NameFirst))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 26;
        }

        if (IsEmpty(import.NewSpouseContact.NameLast))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 27;
        }
      }

      if (Lt(Now().Date, import.New1.MarriageDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 16;
      }

      if (!IsEmpty(import.New1.MarriageCertificateCity) || !
        IsEmpty(import.New1.MarriageCertificateState) || !
        IsEmpty(import.New1.MarriageCountry))
      {
        if (!IsEmpty(import.New1.MarriageCertificateState))
        {
          local.SuppliedCode.CodeName = "STATE CODE";
          local.SuppliedCodeValue.Cdvalue =
            import.New1.MarriageCertificateState ?? Spaces(10);
          UseCabValidateCodeValue2();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
            export.ErrorCodes.Update.EntryErrorCode.Count = 13;
          }
        }
      }

      if (!IsEmpty(import.New1.MarriageCountry))
      {
        local.SuppliedCode.CodeName = "COUNTRY CODE";
        local.SuppliedCodeValue.Cdvalue = import.New1.MarriageCountry ?? Spaces
          (10);
        UseCabValidateCodeValue1();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 17;
        }
        else
        {
          export.MarriageCountry.Description =
            local.SuppliedCodeValue.Description;
        }
      }
      else
      {
        export.MarriageCountry.Description =
          Spaces(CodeValue.Description_MaxLength);
      }

      if (import.New1.SeparationDate != null)
      {
        if (Lt(import.New1.SeparationDate, import.New1.MarriageDate) || Lt
          (Now().Date, import.New1.SeparationDate))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 19;
        }
      }

      if (!IsEmpty(import.New1.DivorcePendingInd) && AsChar
        (import.New1.DivorcePendingInd) != 'Y' && AsChar
        (import.New1.DivorcePendingInd) != 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        export.ErrorCodes.Update.EntryErrorCode.Count = 23;
      }

      if (import.New1.DivorcePetitionDate != null)
      {
        if (Lt(import.New1.DivorcePetitionDate, import.New1.MarriageDate) || Lt
          (import.New1.DivorcePetitionDate, import.New1.SeparationDate) || Lt
          (Now().Date, import.New1.DivorcePetitionDate))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 21;
        }
      }

      if (import.New1.DivorceDate != null)
      {
        if (Lt(import.New1.DivorceDate, import.New1.MarriageDate) || Lt
          (import.New1.DivorceDate, import.New1.SeparationDate) || Lt
          (import.New1.DivorceDate, import.New1.DivorcePetitionDate) || Lt
          (Now().Date, import.New1.DivorceDate))
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 20;
        }
      }

      if (!IsEmpty(import.New1.DivorceCountry) || !
        IsEmpty(import.New1.DivorceCounty) || !
        IsEmpty(import.New1.DivorceState))
      {
        if (!IsEmpty(import.New1.DivorceState))
        {
          local.SuppliedCode.CodeName = "STATE CODE";
          local.SuppliedCodeValue.Cdvalue = import.New1.DivorceState ?? Spaces
            (10);
          UseCabValidateCodeValue2();

          if (AsChar(local.ValidCode.Flag) == 'N')
          {
            ++export.ErrorCodes.Index;
            export.ErrorCodes.CheckSize();

            export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
            export.ErrorCodes.Update.EntryErrorCode.Count = 15;
          }
        }
        else
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 25;
        }
      }

      if (!IsEmpty(import.New1.DivorceCountry))
      {
        local.SuppliedCode.CodeName = "COUNTRY CODE";
        local.SuppliedCodeValue.Cdvalue = import.New1.DivorceCountry ?? Spaces
          (10);
        UseCabValidateCodeValue1();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
          export.ErrorCodes.Update.EntryErrorCode.Count = 18;
        }
        else
        {
          export.DivorceCountry.Description =
            local.SuppliedCodeValue.Description;
        }
      }
      else
      {
        export.DivorceCountry.Description =
          Spaces(CodeValue.Description_MaxLength);
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private void UseCabGetClientDetails1()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingPrime.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.PrimeCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseCabGetClientDetails2()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = export.SpouseCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.SpouseCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.SuppliedCodeValue.Cdvalue;
    useImport.Code.CodeName = local.SuppliedCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    MoveCodeValue(useExport.CodeValue, local.SuppliedCodeValue);
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.SuppliedCodeValue.Cdvalue;
    useImport.Code.CodeName = local.SuppliedCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingNewSpouseCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.NewSpouseCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingNewSpouseCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingNewSpouseCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingPrime.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.PrimeCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPrime.Number = db.GetString(reader, 0);
        entities.ExistingPrime.Populated = true;
      });
  }

  private bool ReadMarriageHistory1()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory1",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetNullableDate(
          command, "marriageDate",
          import.New1.MarriageDate.GetValueOrDefault());
        db.SetInt32(command, "identifier", import.Current.Identifier);
        db.SetNullableString(
          command, "lastName", import.NewSpouseContact.NameLast ?? "");
        db.SetNullableString(
          command, "firstName", import.NewSpouseContact.NameFirst ?? "");
        db.SetNullableString(
          command, "middleInitial", import.NewSpouseContact.MiddleInitial ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.MarriageDate = db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrent.DivorcePetitionDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCurrent.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrent.MarriageCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrent.DivorcePendingInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCurrent.DivorceCounty =
          db.GetNullableString(reader, 7);
        entities.ExistingCurrent.DivorceState = db.GetNullableString(reader, 8);
        entities.ExistingCurrent.DivorceCountry =
          db.GetNullableString(reader, 9);
        entities.ExistingCurrent.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.ExistingCurrent.DivorceDate = db.GetNullableDate(reader, 11);
        entities.ExistingCurrent.SeparationDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCurrent.CreatedBy = db.GetString(reader, 13);
        entities.ExistingCurrent.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ExistingCurrent.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingCurrent.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCurrent.CspNumber = db.GetNullableString(reader, 17);
        entities.ExistingCurrent.CspINumber = db.GetNullableString(reader, 18);
        entities.ExistingCurrent.ConINumber = db.GetNullableInt32(reader, 19);
        entities.ExistingCurrent.DivorceCity = db.GetNullableString(reader, 20);
        entities.ExistingCurrent.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 22);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private bool ReadMarriageHistory2()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory2",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetNullableDate(
          command, "marriageDate",
          import.New1.MarriageDate.GetValueOrDefault());
        db.SetNullableString(
          command, "lastName", import.NewSpouseContact.NameLast ?? "");
        db.SetNullableString(
          command, "firstName", import.NewSpouseContact.NameFirst ?? "");
        db.SetNullableString(
          command, "middleInitial", import.NewSpouseContact.MiddleInitial ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.MarriageDate = db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrent.DivorcePetitionDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCurrent.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrent.MarriageCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrent.DivorcePendingInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCurrent.DivorceCounty =
          db.GetNullableString(reader, 7);
        entities.ExistingCurrent.DivorceState = db.GetNullableString(reader, 8);
        entities.ExistingCurrent.DivorceCountry =
          db.GetNullableString(reader, 9);
        entities.ExistingCurrent.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.ExistingCurrent.DivorceDate = db.GetNullableDate(reader, 11);
        entities.ExistingCurrent.SeparationDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCurrent.CreatedBy = db.GetString(reader, 13);
        entities.ExistingCurrent.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ExistingCurrent.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingCurrent.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCurrent.CspNumber = db.GetNullableString(reader, 17);
        entities.ExistingCurrent.CspINumber = db.GetNullableString(reader, 18);
        entities.ExistingCurrent.ConINumber = db.GetNullableInt32(reader, 19);
        entities.ExistingCurrent.DivorceCity = db.GetNullableString(reader, 20);
        entities.ExistingCurrent.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 22);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private bool ReadMarriageHistory3()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory3",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetNullableDate(
          command, "marriageDate",
          import.New1.MarriageDate.GetValueOrDefault());
        db.SetInt32(command, "identifier", import.Current.Identifier);
        db.SetNullableString(
          command, "cspNumber", import.NewSpouseCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.MarriageDate = db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrent.DivorcePetitionDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCurrent.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrent.MarriageCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrent.DivorcePendingInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCurrent.DivorceCounty =
          db.GetNullableString(reader, 7);
        entities.ExistingCurrent.DivorceState = db.GetNullableString(reader, 8);
        entities.ExistingCurrent.DivorceCountry =
          db.GetNullableString(reader, 9);
        entities.ExistingCurrent.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.ExistingCurrent.DivorceDate = db.GetNullableDate(reader, 11);
        entities.ExistingCurrent.SeparationDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCurrent.CreatedBy = db.GetString(reader, 13);
        entities.ExistingCurrent.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ExistingCurrent.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingCurrent.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCurrent.CspNumber = db.GetNullableString(reader, 17);
        entities.ExistingCurrent.CspINumber = db.GetNullableString(reader, 18);
        entities.ExistingCurrent.ConINumber = db.GetNullableInt32(reader, 19);
        entities.ExistingCurrent.DivorceCity = db.GetNullableString(reader, 20);
        entities.ExistingCurrent.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 22);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private bool ReadMarriageHistory4()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory4",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNumber", entities.ExistingPrime.Number);
          
        db.SetNullableDate(
          command, "marriageDate",
          import.New1.MarriageDate.GetValueOrDefault());
        db.
          SetString(command, "cspRNumber", import.CurrentSpouseCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.MarriageDate = db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrent.DivorcePetitionDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCurrent.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrent.MarriageCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrent.DivorcePendingInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCurrent.DivorceCounty =
          db.GetNullableString(reader, 7);
        entities.ExistingCurrent.DivorceState = db.GetNullableString(reader, 8);
        entities.ExistingCurrent.DivorceCountry =
          db.GetNullableString(reader, 9);
        entities.ExistingCurrent.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.ExistingCurrent.DivorceDate = db.GetNullableDate(reader, 11);
        entities.ExistingCurrent.SeparationDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCurrent.CreatedBy = db.GetString(reader, 13);
        entities.ExistingCurrent.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ExistingCurrent.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingCurrent.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCurrent.CspNumber = db.GetNullableString(reader, 17);
        entities.ExistingCurrent.CspINumber = db.GetNullableString(reader, 18);
        entities.ExistingCurrent.ConINumber = db.GetNullableInt32(reader, 19);
        entities.ExistingCurrent.DivorceCity = db.GetNullableString(reader, 20);
        entities.ExistingCurrent.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 22);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private bool ReadMarriageHistory5()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory5",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNumber", entities.ExistingPrime.Number);
          
        db.SetNullableDate(
          command, "marriageDate",
          import.New1.MarriageDate.GetValueOrDefault());
        db.SetString(command, "cspRNumber", import.NewSpouseCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.MarriageDate = db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrent.DivorcePetitionDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCurrent.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrent.MarriageCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrent.DivorcePendingInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCurrent.DivorceCounty =
          db.GetNullableString(reader, 7);
        entities.ExistingCurrent.DivorceState = db.GetNullableString(reader, 8);
        entities.ExistingCurrent.DivorceCountry =
          db.GetNullableString(reader, 9);
        entities.ExistingCurrent.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.ExistingCurrent.DivorceDate = db.GetNullableDate(reader, 11);
        entities.ExistingCurrent.SeparationDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCurrent.CreatedBy = db.GetString(reader, 13);
        entities.ExistingCurrent.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ExistingCurrent.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingCurrent.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCurrent.CspNumber = db.GetNullableString(reader, 17);
        entities.ExistingCurrent.CspINumber = db.GetNullableString(reader, 18);
        entities.ExistingCurrent.ConINumber = db.GetNullableInt32(reader, 19);
        entities.ExistingCurrent.DivorceCity = db.GetNullableString(reader, 20);
        entities.ExistingCurrent.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 22);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private bool ReadMarriageHistory6()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory6",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetNullableDate(
          command, "marriageDate",
          import.New1.MarriageDate.GetValueOrDefault());
        db.SetNullableString(
          command, "cspNumber", import.NewSpouseCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.MarriageDate = db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrent.DivorcePetitionDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCurrent.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrent.MarriageCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrent.DivorcePendingInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCurrent.DivorceCounty =
          db.GetNullableString(reader, 7);
        entities.ExistingCurrent.DivorceState = db.GetNullableString(reader, 8);
        entities.ExistingCurrent.DivorceCountry =
          db.GetNullableString(reader, 9);
        entities.ExistingCurrent.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.ExistingCurrent.DivorceDate = db.GetNullableDate(reader, 11);
        entities.ExistingCurrent.SeparationDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCurrent.CreatedBy = db.GetString(reader, 13);
        entities.ExistingCurrent.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ExistingCurrent.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingCurrent.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCurrent.CspNumber = db.GetNullableString(reader, 17);
        entities.ExistingCurrent.CspINumber = db.GetNullableString(reader, 18);
        entities.ExistingCurrent.ConINumber = db.GetNullableInt32(reader, 19);
        entities.ExistingCurrent.DivorceCity = db.GetNullableString(reader, 20);
        entities.ExistingCurrent.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 22);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private bool ReadMarriageHistory7()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory7",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetInt32(command, "identifier", import.Current.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.MarriageDate = db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrent.DivorcePetitionDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCurrent.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrent.MarriageCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrent.DivorcePendingInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCurrent.DivorceCounty =
          db.GetNullableString(reader, 7);
        entities.ExistingCurrent.DivorceState = db.GetNullableString(reader, 8);
        entities.ExistingCurrent.DivorceCountry =
          db.GetNullableString(reader, 9);
        entities.ExistingCurrent.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.ExistingCurrent.DivorceDate = db.GetNullableDate(reader, 11);
        entities.ExistingCurrent.SeparationDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCurrent.CreatedBy = db.GetString(reader, 13);
        entities.ExistingCurrent.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ExistingCurrent.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingCurrent.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCurrent.CspNumber = db.GetNullableString(reader, 17);
        entities.ExistingCurrent.CspINumber = db.GetNullableString(reader, 18);
        entities.ExistingCurrent.ConINumber = db.GetNullableInt32(reader, 19);
        entities.ExistingCurrent.DivorceCity = db.GetNullableString(reader, 20);
        entities.ExistingCurrent.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 22);
        entities.ExistingCurrent.Populated = true;
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
    /// A value of Spouse.
    /// </summary>
    [JsonPropertyName("spouse")]
    public CsePersonsWorkSet Spouse
    {
      get => spouse ??= new();
      set => spouse = value;
    }

    /// <summary>
    /// A value of PrimeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("primeCsePersonsWorkSet")]
    public CsePersonsWorkSet PrimeCsePersonsWorkSet
    {
      get => primeCsePersonsWorkSet ??= new();
      set => primeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CurrentSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("currentSpouseCsePerson")]
    public CsePerson CurrentSpouseCsePerson
    {
      get => currentSpouseCsePerson ??= new();
      set => currentSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of CurrentSpouseContact.
    /// </summary>
    [JsonPropertyName("currentSpouseContact")]
    public Contact CurrentSpouseContact
    {
      get => currentSpouseContact ??= new();
      set => currentSpouseContact = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public MarriageHistory Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Standard UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of PrimeCsePerson.
    /// </summary>
    [JsonPropertyName("primeCsePerson")]
    public CsePerson PrimeCsePerson
    {
      get => primeCsePerson ??= new();
      set => primeCsePerson = value;
    }

    /// <summary>
    /// A value of NewSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("newSpouseCsePerson")]
    public CsePerson NewSpouseCsePerson
    {
      get => newSpouseCsePerson ??= new();
      set => newSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of NewSpouseContact.
    /// </summary>
    [JsonPropertyName("newSpouseContact")]
    public Contact NewSpouseContact
    {
      get => newSpouseContact ??= new();
      set => newSpouseContact = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public MarriageHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CsePersonsWorkSet spouse;
    private CsePersonsWorkSet primeCsePersonsWorkSet;
    private CsePerson currentSpouseCsePerson;
    private Contact currentSpouseContact;
    private MarriageHistory current;
    private Standard userAction;
    private CsePerson primeCsePerson;
    private CsePerson newSpouseCsePerson;
    private Contact newSpouseContact;
    private MarriageHistory new1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of EntryErrorCode.
      /// </summary>
      [JsonPropertyName("entryErrorCode")]
      public Common EntryErrorCode
      {
        get => entryErrorCode ??= new();
        set => entryErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common entryErrorCode;
    }

    /// <summary>
    /// A value of SpouseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("spouseCsePersonsWorkSet")]
    public CsePersonsWorkSet SpouseCsePersonsWorkSet
    {
      get => spouseCsePersonsWorkSet ??= new();
      set => spouseCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SpouseCsePerson.
    /// </summary>
    [JsonPropertyName("spouseCsePerson")]
    public CsePerson SpouseCsePerson
    {
      get => spouseCsePerson ??= new();
      set => spouseCsePerson = value;
    }

    /// <summary>
    /// A value of PrimeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("primeCsePersonsWorkSet")]
    public CsePersonsWorkSet PrimeCsePersonsWorkSet
    {
      get => primeCsePersonsWorkSet ??= new();
      set => primeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PrimeCsePerson.
    /// </summary>
    [JsonPropertyName("primeCsePerson")]
    public CsePerson PrimeCsePerson
    {
      get => primeCsePerson ??= new();
      set => primeCsePerson = value;
    }

    /// <summary>
    /// A value of DivorceCountry.
    /// </summary>
    [JsonPropertyName("divorceCountry")]
    public CodeValue DivorceCountry
    {
      get => divorceCountry ??= new();
      set => divorceCountry = value;
    }

    /// <summary>
    /// A value of MarriageCountry.
    /// </summary>
    [JsonPropertyName("marriageCountry")]
    public CodeValue MarriageCountry
    {
      get => marriageCountry ??= new();
      set => marriageCountry = value;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private CsePersonsWorkSet spouseCsePersonsWorkSet;
    private CsePerson spouseCsePerson;
    private CsePersonsWorkSet primeCsePersonsWorkSet;
    private CsePerson primeCsePerson;
    private CodeValue divorceCountry;
    private CodeValue marriageCountry;
    private Common lastErrorEntryNo;
    private Array<ErrorCodesGroup> errorCodes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Local01010001.
    /// </summary>
    [JsonPropertyName("local01010001")]
    public MarriageHistory Local01010001
    {
      get => local01010001 ??= new();
      set => local01010001 = value;
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
    /// A value of SuppliedCodeValue.
    /// </summary>
    [JsonPropertyName("suppliedCodeValue")]
    public CodeValue SuppliedCodeValue
    {
      get => suppliedCodeValue ??= new();
      set => suppliedCodeValue = value;
    }

    /// <summary>
    /// A value of SuppliedCode.
    /// </summary>
    [JsonPropertyName("suppliedCode")]
    public Code SuppliedCode
    {
      get => suppliedCode ??= new();
      set => suppliedCode = value;
    }

    private MarriageHistory local01010001;
    private Common validCode;
    private CodeValue suppliedCodeValue;
    private Code suppliedCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingAnotherContact.
    /// </summary>
    [JsonPropertyName("existingAnotherContact")]
    public Contact ExistingAnotherContact
    {
      get => existingAnotherContact ??= new();
      set => existingAnotherContact = value;
    }

    /// <summary>
    /// A value of ExistingAnotherCsePerson.
    /// </summary>
    [JsonPropertyName("existingAnotherCsePerson")]
    public CsePerson ExistingAnotherCsePerson
    {
      get => existingAnotherCsePerson ??= new();
      set => existingAnotherCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCurrentSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("existingCurrentSpouseCsePerson")]
    public CsePerson ExistingCurrentSpouseCsePerson
    {
      get => existingCurrentSpouseCsePerson ??= new();
      set => existingCurrentSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCurrentSpouseContact.
    /// </summary>
    [JsonPropertyName("existingCurrentSpouseContact")]
    public Contact ExistingCurrentSpouseContact
    {
      get => existingCurrentSpouseContact ??= new();
      set => existingCurrentSpouseContact = value;
    }

    /// <summary>
    /// A value of ExistingCurrent.
    /// </summary>
    [JsonPropertyName("existingCurrent")]
    public MarriageHistory ExistingCurrent
    {
      get => existingCurrent ??= new();
      set => existingCurrent = value;
    }

    /// <summary>
    /// A value of ExistingMarriageCountyCode.
    /// </summary>
    [JsonPropertyName("existingMarriageCountyCode")]
    public Code ExistingMarriageCountyCode
    {
      get => existingMarriageCountyCode ??= new();
      set => existingMarriageCountyCode = value;
    }

    /// <summary>
    /// A value of ExistingMarriageCountyCodeValue.
    /// </summary>
    [JsonPropertyName("existingMarriageCountyCodeValue")]
    public CodeValue ExistingMarriageCountyCodeValue
    {
      get => existingMarriageCountyCodeValue ??= new();
      set => existingMarriageCountyCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingMarriageStateCode.
    /// </summary>
    [JsonPropertyName("existingMarriageStateCode")]
    public Code ExistingMarriageStateCode
    {
      get => existingMarriageStateCode ??= new();
      set => existingMarriageStateCode = value;
    }

    /// <summary>
    /// A value of ExistingMarriageStateCodeValue.
    /// </summary>
    [JsonPropertyName("existingMarriageStateCodeValue")]
    public CodeValue ExistingMarriageStateCodeValue
    {
      get => existingMarriageStateCodeValue ??= new();
      set => existingMarriageStateCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingMarriageCountryCode.
    /// </summary>
    [JsonPropertyName("existingMarriageCountryCode")]
    public Code ExistingMarriageCountryCode
    {
      get => existingMarriageCountryCode ??= new();
      set => existingMarriageCountryCode = value;
    }

    /// <summary>
    /// A value of ExistingMarriageCountryCodeValue.
    /// </summary>
    [JsonPropertyName("existingMarriageCountryCodeValue")]
    public CodeValue ExistingMarriageCountryCodeValue
    {
      get => existingMarriageCountryCodeValue ??= new();
      set => existingMarriageCountryCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingDivorceCountyCode.
    /// </summary>
    [JsonPropertyName("existingDivorceCountyCode")]
    public Code ExistingDivorceCountyCode
    {
      get => existingDivorceCountyCode ??= new();
      set => existingDivorceCountyCode = value;
    }

    /// <summary>
    /// A value of ExistingDivorceCountyCodeValue.
    /// </summary>
    [JsonPropertyName("existingDivorceCountyCodeValue")]
    public CodeValue ExistingDivorceCountyCodeValue
    {
      get => existingDivorceCountyCodeValue ??= new();
      set => existingDivorceCountyCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingDivorceStateCode.
    /// </summary>
    [JsonPropertyName("existingDivorceStateCode")]
    public Code ExistingDivorceStateCode
    {
      get => existingDivorceStateCode ??= new();
      set => existingDivorceStateCode = value;
    }

    /// <summary>
    /// A value of ExistingDivorceStateCodeValue.
    /// </summary>
    [JsonPropertyName("existingDivorceStateCodeValue")]
    public CodeValue ExistingDivorceStateCodeValue
    {
      get => existingDivorceStateCodeValue ??= new();
      set => existingDivorceStateCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingDivorceCountryCode.
    /// </summary>
    [JsonPropertyName("existingDivorceCountryCode")]
    public Code ExistingDivorceCountryCode
    {
      get => existingDivorceCountryCode ??= new();
      set => existingDivorceCountryCode = value;
    }

    /// <summary>
    /// A value of ExistingDivorceCountryCodeValue.
    /// </summary>
    [JsonPropertyName("existingDivorceCountryCodeValue")]
    public CodeValue ExistingDivorceCountryCodeValue
    {
      get => existingDivorceCountryCodeValue ??= new();
      set => existingDivorceCountryCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingPrime.
    /// </summary>
    [JsonPropertyName("existingPrime")]
    public CsePerson ExistingPrime
    {
      get => existingPrime ??= new();
      set => existingPrime = value;
    }

    /// <summary>
    /// A value of ExistingNewSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("existingNewSpouseCsePerson")]
    public CsePerson ExistingNewSpouseCsePerson
    {
      get => existingNewSpouseCsePerson ??= new();
      set => existingNewSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingNewSpouseContact.
    /// </summary>
    [JsonPropertyName("existingNewSpouseContact")]
    public Contact ExistingNewSpouseContact
    {
      get => existingNewSpouseContact ??= new();
      set => existingNewSpouseContact = value;
    }

    private Contact existingAnotherContact;
    private CsePerson existingAnotherCsePerson;
    private CsePerson existingCurrentSpouseCsePerson;
    private Contact existingCurrentSpouseContact;
    private MarriageHistory existingCurrent;
    private Code existingMarriageCountyCode;
    private CodeValue existingMarriageCountyCodeValue;
    private Code existingMarriageStateCode;
    private CodeValue existingMarriageStateCodeValue;
    private Code existingMarriageCountryCode;
    private CodeValue existingMarriageCountryCodeValue;
    private Code existingDivorceCountyCode;
    private CodeValue existingDivorceCountyCodeValue;
    private Code existingDivorceStateCode;
    private CodeValue existingDivorceStateCodeValue;
    private Code existingDivorceCountryCode;
    private CodeValue existingDivorceCountryCodeValue;
    private CsePerson existingPrime;
    private CsePerson existingNewSpouseCsePerson;
    private Contact existingNewSpouseContact;
  }
#endregion
}
