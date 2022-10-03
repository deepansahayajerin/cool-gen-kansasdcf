// Program: LE_EIWH_DISPLAY_EIWO_HISTORY, ID: 1902506333, model: 746.
// Short name: SWE00842
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_EIWH_DISPLAY_EIWO_HISTORY.
/// </summary>
[Serializable]
public partial class LeEiwhDisplayEiwoHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EIWH_DISPLAY_EIWO_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEiwhDisplayEiwoHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEiwhDisplayEiwoHistory.
  /// </summary>
  public LeEiwhDisplayEiwoHistory(IContext context, Import import, Export export)
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
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 08/20/15  GVandy	CQ22212		Initial Code.
    // -------------------------------------------------------------------------------------
    if (ReadIwoTransaction())
    {
      export.IwoTransaction.Assign(entities.IwoTransaction);
    }
    else
    {
      ExitState = "IWO_TRANSACTION_NF";

      return;
    }

    export.Export1.Index = -1;

    foreach(var item in ReadIwoAction())
    {
      export.MostRecent.Assign(entities.IwoAction);

      foreach(var item1 in ReadIwoActionHistory())
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        MoveIwoAction(entities.IwoAction, export.Export1.Update.GiwoAction);

        if (entities.IwoActionHistory.Identifier != 1)
        {
          export.Export1.Update.GiwoAction.DocumentTrackingIdentifier = "";
        }

        MoveIwoActionHistory(entities.IwoActionHistory,
          export.Export1.Update.GiwoActionHistory);

        switch(TrimEnd(entities.IwoActionHistory.ActionTaken))
        {
          case "S":
            if (Equal(entities.IwoAction.ActionType, "E-IWO"))
            {
              export.Export1.Update.GworkArea.Text50 = "Submitted Successfully";
            }
            else if (Equal(entities.IwoAction.ActionType, "RESUB"))
            {
              export.Export1.Update.GworkArea.Text50 =
                "Resubmitted Successfully";
            }

            break;
          case "E":
            if (Equal(entities.IwoActionHistory.CreatedBy, "SWELB588"))
            {
              export.Export1.Update.GworkArea.Text50 = "Portal Error";
            }
            else
            {
              export.Export1.Update.GworkArea.Text50 =
                "Failed Document Field Retrieval";
            }

            break;
          case "N":
            export.Export1.Update.GworkArea.Text50 = "Sent to Portal";

            break;
          case "R":
            export.Export1.Update.GworkArea.Text50 = "Receipted by Portal";

            break;
          case "A":
            export.Export1.Update.GworkArea.Text50 = "Accepted";

            break;
          case "J":
            export.Export1.Update.GworkArea.Text50 = "Rejected";

            break;
          case "C":
            export.Export1.Update.GworkArea.Text50 = "Cancelled";

            break;
          case "Z":
            export.Export1.Update.GworkArea.Text50 = "Severity Cleared";

            break;
          default:
            break;
        }

        if (Equal(entities.IwoActionHistory.ActionTaken, "A") || Equal
          (entities.IwoActionHistory.ActionTaken, "J"))
        {
          switch(TrimEnd(entities.IwoAction.StatusReasonCode))
          {
            case "B":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Name Mismatch";
                

              break;
            case "S":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Employee is in Suspense Status";
                

              break;
            case "W":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Incorrect FEIN";
                

              break;
            case "D":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Duplicate IWO";
                

              break;
            case "M":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - IWO Received from Multiple States";
                

              break;
            case "N":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - NCP No Longer at Employer";
                

              break;
            case "O":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Other Reason";
                

              break;
            case "U":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - NCP Not Known to Employer";
                

              break;
            case "X":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Employer Could Not Process Record";
                

              break;
            case "Z":
              export.Export1.Update.GworkArea.Text50 =
                TrimEnd(export.Export1.Item.GworkArea.Text50) + " - No Current IWO in Place";
                

              break;
            default:
              break;
          }
        }

        if (Equal(entities.IwoActionHistory.ActionTaken, "E") && Equal
          (entities.IwoActionHistory.CreatedBy, "SWELB588"))
        {
          export.Export1.Update.GworkArea.Text50 =
            TrimEnd(export.Export1.Item.GworkArea.Text50) + " - " + entities
            .IwoAction.ErrorRecordType;

          if (Equal(entities.IwoAction.ErrorRecordType, "DTL"))
          {
            export.Export1.Update.GworkArea.Text50 =
              TrimEnd(export.Export1.Item.GworkArea.Text50) + " - " + TrimEnd
              (entities.IwoAction.ErrorField1);
          }
          else
          {
            switch(TrimEnd(entities.IwoAction.StatusReasonCode))
            {
              case "CDT":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " -  Creation Date";
                  

                break;
              case "CNM":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Control Number";
                  

                break;
              case "CTM":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " -  Creation Time";
                  

                break;
              case "DOC":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Document Code";
                  

                break;
              case "DUP":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - File Already Received";
                  

                break;
              case "EIN":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - EIN Text";
                  

                break;
              case "FPS":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - State FIPS Code";
                  

                break;
              case "PPE":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Payroll Processor EIN Text";
                  

                break;
              case "BCT":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Batch Count";
                  

                break;
              case "RCT":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Record Count";
                  

                break;
              case "REC":
                export.Export1.Update.GworkArea.Text50 =
                  TrimEnd(export.Export1.Item.GworkArea.Text50) + " - Invalid File Structure";
                  

                break;
              default:
                break;
            }
          }
        }
      }

      // -- Insert a blank line.
      ++export.Export1.Index;
      export.Export1.CheckSize();
    }
  }

  private static void MoveIwoAction(IwoAction source, IwoAction target)
  {
    target.DocumentTrackingIdentifier = source.DocumentTrackingIdentifier;
    target.SeverityClearedInd = source.SeverityClearedInd;
  }

  private static void MoveIwoActionHistory(IwoActionHistory source,
    IwoActionHistory target)
  {
    target.ActionDate = source.ActionDate;
    target.CreatedBy = source.CreatedBy;
  }

  private IEnumerable<bool> ReadIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.IwoAction.Populated = false;

    return ReadEach("ReadIwoAction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.
          SetInt32(command, "iwtIdentifier", entities.IwoTransaction.Identifier);
          
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoAction.StatusReasonCode = db.GetNullableString(reader, 4);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 5);
        entities.IwoAction.SeverityClearedInd = db.GetNullableString(reader, 6);
        entities.IwoAction.ErrorRecordType = db.GetNullableString(reader, 7);
        entities.IwoAction.ErrorField1 = db.GetNullableString(reader, 8);
        entities.IwoAction.ErrorField2 = db.GetNullableString(reader, 9);
        entities.IwoAction.CspNumber = db.GetString(reader, 10);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 11);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 12);
        entities.IwoAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIwoActionHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    entities.IwoActionHistory.Populated = false;

    return ReadEach("ReadIwoActionHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
        db.SetInt32(command, "iwaIdentifier", entities.IwoAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IwoActionHistory.Identifier = db.GetInt32(reader, 0);
        entities.IwoActionHistory.ActionTaken = db.GetNullableString(reader, 1);
        entities.IwoActionHistory.ActionDate = db.GetNullableDate(reader, 2);
        entities.IwoActionHistory.CreatedBy = db.GetString(reader, 3);
        entities.IwoActionHistory.CspNumber = db.GetString(reader, 4);
        entities.IwoActionHistory.LgaIdentifier = db.GetInt32(reader, 5);
        entities.IwoActionHistory.IwtIdentifier = db.GetInt32(reader, 6);
        entities.IwoActionHistory.IwaIdentifier = db.GetInt32(reader, 7);
        entities.IwoActionHistory.Populated = true;

        return true;
      });
  }

  private bool ReadIwoTransaction()
  {
    entities.IwoTransaction.Populated = false;

    return Read("ReadIwoTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.IwoTransaction.Identifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.Note = db.GetNullableString(reader, 4);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 5);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 6);
        entities.IwoTransaction.Populated = true;
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
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private IwoTransaction iwoTransaction;
    private LegalAction legalAction;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of GiwoAction.
      /// </summary>
      [JsonPropertyName("giwoAction")]
      public IwoAction GiwoAction
      {
        get => giwoAction ??= new();
        set => giwoAction = value;
      }

      /// <summary>
      /// A value of GworkArea.
      /// </summary>
      [JsonPropertyName("gworkArea")]
      public WorkArea GworkArea
      {
        get => gworkArea ??= new();
        set => gworkArea = value;
      }

      /// <summary>
      /// A value of GiwoActionHistory.
      /// </summary>
      [JsonPropertyName("giwoActionHistory")]
      public IwoActionHistory GiwoActionHistory
      {
        get => giwoActionHistory ??= new();
        set => giwoActionHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 96;

      private IwoAction giwoAction;
      private WorkArea gworkArea;
      private IwoActionHistory giwoActionHistory;
    }

    /// <summary>
    /// A value of MostRecent.
    /// </summary>
    [JsonPropertyName("mostRecent")]
    public IwoAction MostRecent
    {
      get => mostRecent ??= new();
      set => mostRecent = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private IwoAction mostRecent;
    private IwoTransaction iwoTransaction;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IwoActionHistory.
    /// </summary>
    [JsonPropertyName("iwoActionHistory")]
    public IwoActionHistory IwoActionHistory
    {
      get => iwoActionHistory ??= new();
      set => iwoActionHistory = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private IwoActionHistory iwoActionHistory;
    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
    private LegalAction legalAction;
    private CsePerson csePerson;
  }
#endregion
}
