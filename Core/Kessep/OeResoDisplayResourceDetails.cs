// Program: OE_RESO_DISPLAY_RESOURCE_DETAILS, ID: 371812138, model: 746.
// Short name: SWE00962
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
/// A program: OE_RESO_DISPLAY_RESOURCE_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Using CSE person number, read the entities of CSE PERSON, CSE PERSON 
/// RESOURCE, RESOURCE LOCATION ADDRESS, RESOURCE LIEN HOLDER ADDRESS,
/// LEGAL_ACTION_PERSON_RESOURCE, and move the resource information to an export
/// area.
/// </para>
/// </summary>
[Serializable]
public partial class OeResoDisplayResourceDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_RESO_DISPLAY_RESOURCE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeResoDisplayResourceDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeResoDisplayResourceDetails.
  /// </summary>
  public OeResoDisplayResourceDetails(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	 CHG REQ# DESCRIPTION
    // unknown		MM/DD/YY  	  Initial Code
    // T.O.Redmond	12/28/95          Enhancement
    // Space out the CSE Person Lienholder name and
    // address if an active legal action person resource
    // is found.
    // G.Lofton	03/01/96	  Removed
    // enhancement, made necessary changes for problems
    // encountered during unit testing.
    // G.Lofton	03/18/96	  Retrieve
    // external agency if one exists.
    // R. Welborn      11/05/96         Reset Read Each in Display to retain 
    // currency for later reads.
    // A.Kinney	05/01/97	Changed Current_Date
    // ******** END MAINTENANCE LOG ****************
    local.Current.Date = Now().Date;
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonResource.ResourceNo = import.CsePersonResource.ResourceNo;
    local.Search.ResourceNo = import.CsePersonResource.ResourceNo;
    export.CsePersonVehicle.Identifier =
      import.StartCsePersonVehicle.Identifier;
    UseOeCabSetMnemonics();

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.ExistingCsePerson.Number;
      export.CsePersonsWorkSet.Number = entities.ExistingCsePerson.Number;
      UseSiReadCsePerson();
    }
    else
    {
      export.CsePersonResource.ResourceNo = 0;
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.ResourceFound.Flag = "N";

    if (Lt(local.Initialize.Identifier, import.StartIncomeSource.Identifier))
    {
      if (ReadIncomeSourceCsePersonResource())
      {
        local.Search.ResourceNo = entities.ExistingCsePersonResource.ResourceNo;
      }
    }

    if (import.StartCsePersonVehicle.Identifier > 0)
    {
      if (ReadCsePersonVehicleCsePersonResource())
      {
        local.Search.ResourceNo = entities.ExistingCsePersonResource.ResourceNo;
      }
      else
      {
        ExitState = "OE0000_NO_RESO_FOR_CARS";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (local.Search.ResourceNo > 0)
        {
          if (ReadCsePersonResource2())
          {
            local.ResourceFound.Flag = "Y";
            export.CsePersonResource.Assign(entities.ExistingCsePersonResource);
          }
        }
        else if (ReadCsePersonResource4())
        {
          local.ResourceFound.Flag = "Y";
          export.CsePersonResource.Assign(entities.ExistingCsePersonResource);
        }

        if (AsChar(local.ResourceFound.Flag) == 'N')
        {
          ExitState = "OE0000_NO_RESOURCE_FOUND";

          return;
        }

        break;
      case "PREV":
        if (ReadCsePersonResource3())
        {
          local.ResourceFound.Flag = "Y";
          export.CsePersonResource.Assign(entities.ExistingCsePersonResource);
        }

        if (AsChar(local.ResourceFound.Flag) == 'N')
        {
          ExitState = "OE0000_NO_MORE_RESOURCE_2_SCROLL";
        }

        break;
      case "NEXT":
        if (ReadCsePersonResource1())
        {
          local.ResourceFound.Flag = "Y";
          export.CsePersonResource.Assign(entities.ExistingCsePersonResource);
        }

        if (AsChar(local.ResourceFound.Flag) == 'N')
        {
          ExitState = "OE0000_NO_MORE_RESOURCE_2_SCROLL";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    export.LastUpdated.LastUpdatedBy = export.CsePersonResource.CreatedBy;
    export.LastUpdated.LastUpdatedTimestamp =
      export.CsePersonResource.CreatedTimestamp;

    if (Lt(export.LastUpdated.LastUpdatedTimestamp,
      export.CsePersonResource.LastUpdatedTimestamp))
    {
      export.LastUpdated.LastUpdatedBy = export.CsePersonResource.LastUpdatedBy;
      export.LastUpdated.LastUpdatedTimestamp =
        export.CsePersonResource.LastUpdatedTimestamp;
    }

    local.Code.CodeName = "RESOURCE TYPE";
    local.CodeValue.Cdvalue = entities.ExistingCsePersonResource.Type1 ?? Spaces
      (10);
    UseCabGetCodeValueDescription();
    export.ResourceTypeDesc.Description = local.CodeValue.Description;

    foreach(var item in ReadResourceLocationAddress())
    {
      export.ResourceLocationAddress.Assign(
        entities.ExistingResourceLocationAddress);

      if (Lt(export.LastUpdated.LastUpdatedTimestamp,
        export.ResourceLocationAddress.CreatedTimestamp))
      {
        export.LastUpdated.LastUpdatedBy =
          export.ResourceLocationAddress.CreatedBy;
        export.LastUpdated.LastUpdatedTimestamp =
          export.ResourceLocationAddress.CreatedTimestamp;
      }

      if (Lt(export.LastUpdated.LastUpdatedTimestamp,
        export.ResourceLocationAddress.LastUpdatedTimestamp))
      {
        export.LastUpdated.LastUpdatedBy =
          export.ResourceLocationAddress.LastUpdatedBy;
        export.LastUpdated.LastUpdatedTimestamp =
          export.ResourceLocationAddress.LastUpdatedTimestamp;
      }
    }

    if (entities.ExistingCsePersonResource.Populated)
    {
      if (ReadExternalAgency())
      {
        MoveExternalAgency(entities.ExistingExternalAgency,
          export.ExternalAgency);
      }
      else
      {
        export.ExternalAgency.Name = "";
        export.ExternalAgency.Identifier = 0;
      }

      if (ReadCsePersonVehicle())
      {
        export.CsePersonVehicle.Identifier =
          entities.ExistingCsePersonVehicle.Identifier;
      }
    }
    else
    {
      ExitState = "CSE_PERSON_RESOURCE_NF";
    }

    if (ReadLegalActionPersonResource())
    {
      local.LegalResourceFound.Flag = "Y";
      export.LegalActionPersonResource.Assign(
        entities.ExistingLegalActionPersonResource);

      if (Equal(entities.ExistingLegalActionPersonResource.EndDate,
        local.MaxDate.ExpirationDate))
      {
        export.LegalActionPersonResource.EndDate = null;
      }
    }

    if (AsChar(local.LegalResourceFound.Flag) == 'Y')
    {
      export.CsePersonResource.LienHolderStateOfKansasInd = "Y";

      if (AsChar(entities.ExistingCsePersonResource.CseActionTakenCode) == 'L')
      {
        export.CseActionDesc.Description = "LIEN";
        local.Code.CodeName = "LEGAL ACTION LIEN TYPE";
        local.CodeValue.Cdvalue =
          entities.ExistingLegalActionPersonResource.LienType ?? Spaces(10);
        UseCabGetCodeValueDescription();
        export.LegalActionLienType.Description = local.CodeValue.Description;
      }
    }
    else
    {
      export.CsePersonResource.CseActionTakenCode = "";
      export.LegalActionPersonResource.EffectiveDate = null;
      export.LegalActionPersonResource.EndDate = null;
      export.CsePersonResource.LienHolderStateOfKansasInd = "N";
      export.CseActionDesc.Description =
        Spaces(CodeValue.Description_MaxLength);
      export.LegalActionLienType.Description =
        Spaces(CodeValue.Description_MaxLength);
    }

    if (ReadResourceLienHolderAddress())
    {
      export.ResourceLienHolderAddress.Assign(
        entities.ExistingResourceLienHolderAddress);

      if (Lt(export.LastUpdated.LastUpdatedTimestamp,
        export.ResourceLienHolderAddress.CreatedTimestamp))
      {
        export.LastUpdated.LastUpdatedBy =
          export.ResourceLienHolderAddress.CreatedBy;
        export.LastUpdated.LastUpdatedTimestamp =
          export.ResourceLienHolderAddress.CreatedTimestamp;
      }

      if (Lt(export.LastUpdated.LastUpdatedTimestamp,
        export.ResourceLienHolderAddress.LastUpdatedTimestamp))
      {
        export.LastUpdated.LastUpdatedBy =
          export.ResourceLienHolderAddress.LastUpdatedBy;
        export.LastUpdated.LastUpdatedTimestamp =
          export.ResourceLienHolderAddress.LastUpdatedTimestamp;
      }
    }

    if (ReadCsePersonResource6())
    {
      export.ScrollingAttributes.MinusFlag = "-";
    }

    if (ReadCsePersonResource5())
    {
      export.ScrollingAttributes.PlusFlag = "+";
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveExternalAgency(ExternalAgency source,
    ExternalAgency target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    MoveCode(local.Code, useImport.Code);
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    local.ErrorInDecoding.Flag = useExport.ErrorInDecoding.Flag;
    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonResource1()
  {
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource1",
      (db, command) =>
      {
        db.SetInt32(command, "resourceNo", local.Search.ResourceNo);
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCsePersonResource.LocationCounty =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonResource.OtherLienHolderName =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.CoOwnerName =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.VerifiedUserId =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingCsePersonResource.VerifiedDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 10);
        entities.ExistingCsePersonResource.AccountHolderName =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePersonResource.AccountBalance =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCsePersonResource.AccountNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 14);
        entities.ExistingCsePersonResource.Location =
          db.GetNullableString(reader, 15);
        entities.ExistingCsePersonResource.Value =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 18);
        entities.ExistingCsePersonResource.CreatedBy = db.GetString(reader, 19);
        entities.ExistingCsePersonResource.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.ExistingCsePersonResource.LastUpdatedBy =
          db.GetString(reader, 21);
        entities.ExistingCsePersonResource.LastUpdatedTimestamp =
          db.GetDateTime(reader, 22);
        entities.ExistingCsePersonResource.ExaId =
          db.GetNullableInt32(reader, 23);
        entities.ExistingCsePersonResource.OtherLienPlacedDate =
          db.GetNullableDate(reader, 24);
        entities.ExistingCsePersonResource.OtherLienRemovedDate =
          db.GetNullableDate(reader, 25);
        entities.ExistingCsePersonResource.Populated = true;
      });
  }

  private bool ReadCsePersonResource2()
  {
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "resourceNo", local.Search.ResourceNo);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCsePersonResource.LocationCounty =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonResource.OtherLienHolderName =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.CoOwnerName =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.VerifiedUserId =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingCsePersonResource.VerifiedDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 10);
        entities.ExistingCsePersonResource.AccountHolderName =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePersonResource.AccountBalance =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCsePersonResource.AccountNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 14);
        entities.ExistingCsePersonResource.Location =
          db.GetNullableString(reader, 15);
        entities.ExistingCsePersonResource.Value =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 18);
        entities.ExistingCsePersonResource.CreatedBy = db.GetString(reader, 19);
        entities.ExistingCsePersonResource.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.ExistingCsePersonResource.LastUpdatedBy =
          db.GetString(reader, 21);
        entities.ExistingCsePersonResource.LastUpdatedTimestamp =
          db.GetDateTime(reader, 22);
        entities.ExistingCsePersonResource.ExaId =
          db.GetNullableInt32(reader, 23);
        entities.ExistingCsePersonResource.OtherLienPlacedDate =
          db.GetNullableDate(reader, 24);
        entities.ExistingCsePersonResource.OtherLienRemovedDate =
          db.GetNullableDate(reader, 25);
        entities.ExistingCsePersonResource.Populated = true;
      });
  }

  private bool ReadCsePersonResource3()
  {
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource3",
      (db, command) =>
      {
        db.SetInt32(command, "resourceNo", local.Search.ResourceNo);
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCsePersonResource.LocationCounty =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonResource.OtherLienHolderName =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.CoOwnerName =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.VerifiedUserId =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingCsePersonResource.VerifiedDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 10);
        entities.ExistingCsePersonResource.AccountHolderName =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePersonResource.AccountBalance =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCsePersonResource.AccountNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 14);
        entities.ExistingCsePersonResource.Location =
          db.GetNullableString(reader, 15);
        entities.ExistingCsePersonResource.Value =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 18);
        entities.ExistingCsePersonResource.CreatedBy = db.GetString(reader, 19);
        entities.ExistingCsePersonResource.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.ExistingCsePersonResource.LastUpdatedBy =
          db.GetString(reader, 21);
        entities.ExistingCsePersonResource.LastUpdatedTimestamp =
          db.GetDateTime(reader, 22);
        entities.ExistingCsePersonResource.ExaId =
          db.GetNullableInt32(reader, 23);
        entities.ExistingCsePersonResource.OtherLienPlacedDate =
          db.GetNullableDate(reader, 24);
        entities.ExistingCsePersonResource.OtherLienRemovedDate =
          db.GetNullableDate(reader, 25);
        entities.ExistingCsePersonResource.Populated = true;
      });
  }

  private bool ReadCsePersonResource4()
  {
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCsePersonResource.LocationCounty =
          db.GetNullableString(reader, 2);
        entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonResource.OtherLienHolderName =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.CoOwnerName =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.VerifiedUserId =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingCsePersonResource.VerifiedDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 10);
        entities.ExistingCsePersonResource.AccountHolderName =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePersonResource.AccountBalance =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingCsePersonResource.AccountNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 14);
        entities.ExistingCsePersonResource.Location =
          db.GetNullableString(reader, 15);
        entities.ExistingCsePersonResource.Value =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingCsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 18);
        entities.ExistingCsePersonResource.CreatedBy = db.GetString(reader, 19);
        entities.ExistingCsePersonResource.CreatedTimestamp =
          db.GetDateTime(reader, 20);
        entities.ExistingCsePersonResource.LastUpdatedBy =
          db.GetString(reader, 21);
        entities.ExistingCsePersonResource.LastUpdatedTimestamp =
          db.GetDateTime(reader, 22);
        entities.ExistingCsePersonResource.ExaId =
          db.GetNullableInt32(reader, 23);
        entities.ExistingCsePersonResource.OtherLienPlacedDate =
          db.GetNullableDate(reader, 24);
        entities.ExistingCsePersonResource.OtherLienRemovedDate =
          db.GetNullableDate(reader, 25);
        entities.ExistingCsePersonResource.Populated = true;
      });
  }

  private bool ReadCsePersonResource5()
  {
    entities.ExistingPrevOrNext.Populated = false;

    return Read("ReadCsePersonResource5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "resourceNo", export.CsePersonResource.ResourceNo);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNext.CspNumber = db.GetString(reader, 0);
        entities.ExistingPrevOrNext.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingPrevOrNext.Populated = true;
      });
  }

  private bool ReadCsePersonResource6()
  {
    entities.ExistingPrevOrNext.Populated = false;

    return Read("ReadCsePersonResource6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "resourceNo", export.CsePersonResource.ResourceNo);
      },
      (db, reader) =>
      {
        entities.ExistingPrevOrNext.CspNumber = db.GetString(reader, 0);
        entities.ExistingPrevOrNext.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingPrevOrNext.Populated = true;
      });
  }

  private bool ReadCsePersonVehicle()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingCsePersonVehicle.Populated = false;

    return Read("ReadCsePersonVehicle",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cprCResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetNullableString(
          command, "cspCNumber", entities.ExistingCsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonVehicle.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonVehicle.Identifier = db.GetInt32(reader, 1);
        entities.ExistingCsePersonVehicle.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.ExistingCsePersonVehicle.CspCNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonVehicle.Populated = true;
      });
  }

  private bool ReadCsePersonVehicleCsePersonResource()
  {
    entities.ExistingCsePersonResource.Populated = false;
    entities.ExistingCsePersonVehicle.Populated = false;

    return Read("ReadCsePersonVehicleCsePersonResource",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "identifier", import.StartCsePersonVehicle.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonVehicle.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonVehicle.Identifier = db.GetInt32(reader, 1);
        entities.ExistingCsePersonVehicle.CprCResourceNo =
          db.GetNullableInt32(reader, 2);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 2);
        entities.ExistingCsePersonVehicle.CspCNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 3);
        entities.ExistingCsePersonResource.LocationCounty =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.OtherLienHolderName =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonResource.CoOwnerName =
          db.GetNullableString(reader, 7);
        entities.ExistingCsePersonResource.VerifiedUserId =
          db.GetNullableString(reader, 8);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingCsePersonResource.VerifiedDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 11);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 12);
        entities.ExistingCsePersonResource.AccountHolderName =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonResource.AccountBalance =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingCsePersonResource.AccountNumber =
          db.GetNullableString(reader, 15);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 16);
        entities.ExistingCsePersonResource.Location =
          db.GetNullableString(reader, 17);
        entities.ExistingCsePersonResource.Value =
          db.GetNullableDecimal(reader, 18);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 19);
        entities.ExistingCsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 20);
        entities.ExistingCsePersonResource.CreatedBy = db.GetString(reader, 21);
        entities.ExistingCsePersonResource.CreatedTimestamp =
          db.GetDateTime(reader, 22);
        entities.ExistingCsePersonResource.LastUpdatedBy =
          db.GetString(reader, 23);
        entities.ExistingCsePersonResource.LastUpdatedTimestamp =
          db.GetDateTime(reader, 24);
        entities.ExistingCsePersonResource.ExaId =
          db.GetNullableInt32(reader, 25);
        entities.ExistingCsePersonResource.OtherLienPlacedDate =
          db.GetNullableDate(reader, 26);
        entities.ExistingCsePersonResource.OtherLienRemovedDate =
          db.GetNullableDate(reader, 27);
        entities.ExistingCsePersonResource.Populated = true;
        entities.ExistingCsePersonVehicle.Populated = true;
      });
  }

  private bool ReadExternalAgency()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingExternalAgency.Populated = false;

    return Read("ReadExternalAgency",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingCsePersonResource.ExaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingExternalAgency.Identifier = db.GetInt32(reader, 0);
        entities.ExistingExternalAgency.Name = db.GetString(reader, 1);
        entities.ExistingExternalAgency.Populated = true;
      });
  }

  private bool ReadIncomeSourceCsePersonResource()
  {
    entities.ExistingIncomeSource.Populated = false;
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadIncomeSourceCsePersonResource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.ExistingCsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.StartIncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 2);
        entities.ExistingIncomeSource.CprResourceNo =
          db.GetNullableInt32(reader, 3);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 3);
        entities.ExistingIncomeSource.CspNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 4);
        entities.ExistingCsePersonResource.LocationCounty =
          db.GetNullableString(reader, 5);
        entities.ExistingCsePersonResource.LienHolderStateOfKansasInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCsePersonResource.OtherLienHolderName =
          db.GetNullableString(reader, 7);
        entities.ExistingCsePersonResource.CoOwnerName =
          db.GetNullableString(reader, 8);
        entities.ExistingCsePersonResource.VerifiedUserId =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePersonResource.ResourceDisposalDate =
          db.GetNullableDate(reader, 10);
        entities.ExistingCsePersonResource.VerifiedDate =
          db.GetNullableDate(reader, 11);
        entities.ExistingCsePersonResource.LienIndicator =
          db.GetNullableString(reader, 12);
        entities.ExistingCsePersonResource.Type1 =
          db.GetNullableString(reader, 13);
        entities.ExistingCsePersonResource.AccountHolderName =
          db.GetNullableString(reader, 14);
        entities.ExistingCsePersonResource.AccountBalance =
          db.GetNullableDecimal(reader, 15);
        entities.ExistingCsePersonResource.AccountNumber =
          db.GetNullableString(reader, 16);
        entities.ExistingCsePersonResource.ResourceDescription =
          db.GetNullableString(reader, 17);
        entities.ExistingCsePersonResource.Location =
          db.GetNullableString(reader, 18);
        entities.ExistingCsePersonResource.Value =
          db.GetNullableDecimal(reader, 19);
        entities.ExistingCsePersonResource.Equity =
          db.GetNullableDecimal(reader, 20);
        entities.ExistingCsePersonResource.CseActionTakenCode =
          db.GetNullableString(reader, 21);
        entities.ExistingCsePersonResource.CreatedBy = db.GetString(reader, 22);
        entities.ExistingCsePersonResource.CreatedTimestamp =
          db.GetDateTime(reader, 23);
        entities.ExistingCsePersonResource.LastUpdatedBy =
          db.GetString(reader, 24);
        entities.ExistingCsePersonResource.LastUpdatedTimestamp =
          db.GetDateTime(reader, 25);
        entities.ExistingCsePersonResource.ExaId =
          db.GetNullableInt32(reader, 26);
        entities.ExistingCsePersonResource.OtherLienPlacedDate =
          db.GetNullableDate(reader, 27);
        entities.ExistingCsePersonResource.OtherLienRemovedDate =
          db.GetNullableDate(reader, 28);
        entities.ExistingIncomeSource.Populated = true;
        entities.ExistingCsePersonResource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ExistingIncomeSource.Type1);
      });
  }

  private bool ReadLegalActionPersonResource()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingLegalActionPersonResource.Populated = false;

    return Read("ReadLegalActionPersonResource",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPersonResource.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingLegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 1);
        entities.ExistingLegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingLegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingLegalActionPersonResource.LienType =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionPersonResource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingLegalActionPersonResource.Identifier =
          db.GetInt32(reader, 6);
        entities.ExistingLegalActionPersonResource.Populated = true;
      });
  }

  private bool ReadResourceLienHolderAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingResourceLienHolderAddress.Populated = false;

    return Read("ReadResourceLienHolderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingResourceLienHolderAddress.CprResourceNo =
          db.GetInt32(reader, 0);
        entities.ExistingResourceLienHolderAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingResourceLienHolderAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingResourceLienHolderAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingResourceLienHolderAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingResourceLienHolderAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingResourceLienHolderAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingResourceLienHolderAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingResourceLienHolderAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingResourceLienHolderAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingResourceLienHolderAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingResourceLienHolderAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingResourceLienHolderAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingResourceLienHolderAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingResourceLienHolderAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingResourceLienHolderAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingResourceLienHolderAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingResourceLienHolderAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingResourceLienHolderAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadResourceLocationAddress()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonResource.Populated);
    entities.ExistingResourceLocationAddress.Populated = false;

    return ReadEach("ReadResourceLocationAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "cprResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonResource.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingResourceLocationAddress.CprResourceNo =
          db.GetInt32(reader, 0);
        entities.ExistingResourceLocationAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingResourceLocationAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingResourceLocationAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingResourceLocationAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingResourceLocationAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingResourceLocationAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingResourceLocationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingResourceLocationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingResourceLocationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingResourceLocationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingResourceLocationAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingResourceLocationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingResourceLocationAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingResourceLocationAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingResourceLocationAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingResourceLocationAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingResourceLocationAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingResourceLocationAddress.Populated = true;

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
    /// A value of StartCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("startCsePersonVehicle")]
    public CsePersonVehicle StartCsePersonVehicle
    {
      get => startCsePersonVehicle ??= new();
      set => startCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of StartIncomeSource.
    /// </summary>
    [JsonPropertyName("startIncomeSource")]
    public IncomeSource StartIncomeSource
    {
      get => startIncomeSource ??= new();
      set => startIncomeSource = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePersonVehicle startCsePersonVehicle;
    private IncomeSource startIncomeSource;
    private Common userAction;
    private CsePerson csePerson;
    private CsePersonResource csePersonResource;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LegalActionLienType.
    /// </summary>
    [JsonPropertyName("legalActionLienType")]
    public CodeValue LegalActionLienType
    {
      get => legalActionLienType ??= new();
      set => legalActionLienType = value;
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
    /// A value of LastUpdated.
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public CsePersonResource LastUpdated
    {
      get => lastUpdated ??= new();
      set => lastUpdated = value;
    }

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
    /// A value of CseActionDesc.
    /// </summary>
    [JsonPropertyName("cseActionDesc")]
    public CodeValue CseActionDesc
    {
      get => cseActionDesc ??= new();
      set => cseActionDesc = value;
    }

    /// <summary>
    /// A value of ResourceTypeDesc.
    /// </summary>
    [JsonPropertyName("resourceTypeDesc")]
    public CodeValue ResourceTypeDesc
    {
      get => resourceTypeDesc ??= new();
      set => resourceTypeDesc = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of ResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("resourceLocationAddress")]
    public ResourceLocationAddress ResourceLocationAddress
    {
      get => resourceLocationAddress ??= new();
      set => resourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("resourceLienHolderAddress")]
    public ResourceLienHolderAddress ResourceLienHolderAddress
    {
      get => resourceLienHolderAddress ??= new();
      set => resourceLienHolderAddress = value;
    }

    /// <summary>
    /// A value of CsePersonVehicle.
    /// </summary>
    [JsonPropertyName("csePersonVehicle")]
    public CsePersonVehicle CsePersonVehicle
    {
      get => csePersonVehicle ??= new();
      set => csePersonVehicle = value;
    }

    /// <summary>
    /// A value of ExternalAgency.
    /// </summary>
    [JsonPropertyName("externalAgency")]
    public ExternalAgency ExternalAgency
    {
      get => externalAgency ??= new();
      set => externalAgency = value;
    }

    private CodeValue legalActionLienType;
    private LegalActionPersonResource legalActionPersonResource;
    private CsePersonResource lastUpdated;
    private ScrollingAttributes scrollingAttributes;
    private CodeValue cseActionDesc;
    private CodeValue resourceTypeDesc;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private CsePersonResource csePersonResource;
    private ResourceLocationAddress resourceLocationAddress;
    private ResourceLienHolderAddress resourceLienHolderAddress;
    private CsePersonVehicle csePersonVehicle;
    private ExternalAgency externalAgency;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonResource Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public IncomeSource Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of LegalResourceFound.
    /// </summary>
    [JsonPropertyName("legalResourceFound")]
    public Common LegalResourceFound
    {
      get => legalResourceFound ??= new();
      set => legalResourceFound = value;
    }

    /// <summary>
    /// A value of ErrorInDecoding.
    /// </summary>
    [JsonPropertyName("errorInDecoding")]
    public Common ErrorInDecoding
    {
      get => errorInDecoding ??= new();
      set => errorInDecoding = value;
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
    /// A value of ResourceFound.
    /// </summary>
    [JsonPropertyName("resourceFound")]
    public Common ResourceFound
    {
      get => resourceFound ??= new();
      set => resourceFound = value;
    }

    private DateWorkArea current;
    private CsePersonResource search;
    private IncomeSource initialize;
    private Code maxDate;
    private Common legalResourceFound;
    private Common errorInDecoding;
    private Code code;
    private CodeValue codeValue;
    private Common resourceFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionPersonResource.
    /// </summary>
    [JsonPropertyName("existingLegalActionPersonResource")]
    public LegalActionPersonResource ExistingLegalActionPersonResource
    {
      get => existingLegalActionPersonResource ??= new();
      set => existingLegalActionPersonResource = value;
    }

    /// <summary>
    /// A value of ExistingPrevOrNext.
    /// </summary>
    [JsonPropertyName("existingPrevOrNext")]
    public CsePersonResource ExistingPrevOrNext
    {
      get => existingPrevOrNext ??= new();
      set => existingPrevOrNext = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonResource.
    /// </summary>
    [JsonPropertyName("existingCsePersonResource")]
    public CsePersonResource ExistingCsePersonResource
    {
      get => existingCsePersonResource ??= new();
      set => existingCsePersonResource = value;
    }

    /// <summary>
    /// A value of ExistingResourceLocationAddress.
    /// </summary>
    [JsonPropertyName("existingResourceLocationAddress")]
    public ResourceLocationAddress ExistingResourceLocationAddress
    {
      get => existingResourceLocationAddress ??= new();
      set => existingResourceLocationAddress = value;
    }

    /// <summary>
    /// A value of ExistingResourceLienHolderAddress.
    /// </summary>
    [JsonPropertyName("existingResourceLienHolderAddress")]
    public ResourceLienHolderAddress ExistingResourceLienHolderAddress
    {
      get => existingResourceLienHolderAddress ??= new();
      set => existingResourceLienHolderAddress = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonVehicle.
    /// </summary>
    [JsonPropertyName("existingCsePersonVehicle")]
    public CsePersonVehicle ExistingCsePersonVehicle
    {
      get => existingCsePersonVehicle ??= new();
      set => existingCsePersonVehicle = value;
    }

    /// <summary>
    /// A value of ExistingExternalAgency.
    /// </summary>
    [JsonPropertyName("existingExternalAgency")]
    public ExternalAgency ExistingExternalAgency
    {
      get => existingExternalAgency ??= new();
      set => existingExternalAgency = value;
    }

    private IncomeSource existingIncomeSource;
    private LegalActionPersonResource existingLegalActionPersonResource;
    private CsePersonResource existingPrevOrNext;
    private CsePerson existingCsePerson;
    private CsePersonResource existingCsePersonResource;
    private ResourceLocationAddress existingResourceLocationAddress;
    private ResourceLienHolderAddress existingResourceLienHolderAddress;
    private CsePersonVehicle existingCsePersonVehicle;
    private ExternalAgency existingExternalAgency;
  }
#endregion
}
