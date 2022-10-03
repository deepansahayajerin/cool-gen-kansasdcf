// Program: OE_LGRQ_SET_SCROLL_INDICATORS, ID: 373376339, model: 746.
// Short name: SWE02085
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_LGRQ_SET_SCROLL_INDICATORS.
/// </summary>
[Serializable]
public partial class OeLgrqSetScrollIndicators: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_LGRQ_SET_SCROLL_INDICATORS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeLgrqSetScrollIndicators(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeLgrqSetScrollIndicators.
  /// </summary>
  public OeLgrqSetScrollIndicators(IContext context, Import import,
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
    // ----------------------------------------------------------------------------------------------------------------
    // 		                              
    // P U R P O S E
    // The purpose of this cab is to set the scrolling indicators ( - +) for 
    // display on LGRQ.  This
    // new action block was created because we now sort by Sent and Open 
    // referrals first, then
    // Closed, Rejected, and Withdrawn referrals.  So if an Open referral is 
    // updated to Closed
    // its scrolling position relative to other referrals on the case may have 
    // changed.  For example,
    // assume there are 2 Open referrals on a case.  If the most recent referral
    // is displayed
    // the "-" indicator will be set.  If the referral is then closed then the "
    // -" indicator should be
    // cleared and the "+" indicator should be set.
    // ----------------------------------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // DATE	  DEVELOPER	REQUEST #	DESCRIPTION
    // --------  ------------	--------------	
    // ------------------------------------------------------------------------
    // 04/08/02  GVandy	WR 20184	Initial Development
    // ----------------------------------------------------------------------------------------------------------------
    // ---------------------------------------------
    // Set scroll indicator '-' attribute
    // ---------------------------------------------
    if (AsChar(import.LegalReferral.Status) == 'S' || AsChar
      (import.LegalReferral.Status) == 'O')
    {
      // -- Read for a previous referral in Sent or Open status.
      if (ReadLegalReferral3())
      {
        export.MultipleActiveReferrals.Flag = "Y";
        export.ScrollingAttributes.MinusFlag = "-";

        goto Test;
      }

      // -- If there are no previous referrals in Sent or Open status then read 
      // for referrals in
      //    Closed, Rejected, or Withdrawn status.
      if (ReadLegalReferral6())
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }
    }
    else
    {
      // -- Read for a previous referral in Closed, Rejected, or Withdrawn 
      // status.
      if (ReadLegalReferral4())
      {
        export.ScrollingAttributes.MinusFlag = "-";
      }
    }

Test:

    // ---------------------------------------------
    // Set scroll indicator '+' attribute
    // ---------------------------------------------
    if (AsChar(import.LegalReferral.Status) == 'S' || AsChar
      (import.LegalReferral.Status) == 'O')
    {
      // -- Read for a more recent referral in Sent or Open status.
      if (ReadLegalReferral1())
      {
        export.MultipleActiveReferrals.Flag = "Y";
        export.ScrollingAttributes.PlusFlag = "+";
      }
    }
    else
    {
      // -- Read for a more recent referral in Closed, Rejected, or Withdrawn 
      // status first.
      if (ReadLegalReferral2())
      {
        export.ScrollingAttributes.PlusFlag = "+";

        return;
      }

      // -- If there are no more referrals in Closed, Rejected, or Withdrawn 
      // status then read for
      //    referrals in Sent or Open status.
      if (ReadLegalReferral5())
      {
        export.ScrollingAttributes.PlusFlag = "+";
      }
    }
  }

  private bool ReadLegalReferral1()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral2()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral3()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral4()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetInt32(command, "identifier", import.LegalReferral.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral5()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadLegalReferral6()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.Status = db.GetNullableString(reader, 2);
        entities.LegalReferral.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    private Case1 case1;
    private LegalReferral legalReferral;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MultipleActiveReferrals.
    /// </summary>
    [JsonPropertyName("multipleActiveReferrals")]
    public Common MultipleActiveReferrals
    {
      get => multipleActiveReferrals ??= new();
      set => multipleActiveReferrals = value;
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

    private Common multipleActiveReferrals;
    private ScrollingAttributes scrollingAttributes;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
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

    private LegalReferral legalReferral;
    private Case1 case1;
  }
#endregion
}
