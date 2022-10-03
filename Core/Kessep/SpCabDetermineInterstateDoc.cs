// Program: SP_CAB_DETERMINE_INTERSTATE_DOC, ID: 373018927, model: 746.
// Short name: SWE02738
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
/// A program: SP_CAB_DETERMINE_INTERSTATE_DOC.
/// </para>
/// <para>
/// This CAB determines whether to send an Interstate version of a specific 
/// document versus the non-Interstate version
/// </para>
/// </summary>
[Serializable]
public partial class SpCabDetermineInterstateDoc: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DETERMINE_INTERSTATE_DOC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDetermineInterstateDoc(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDetermineInterstateDoc.
  /// </summary>
  public SpCabDetermineInterstateDoc(IContext context, Import import,
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
    // ----------------------------------------------------
    // Date		Developer	Request
    // ----------------------------------------------------
    // 08/02/2001	M Ramirez	88733
    // Send Interstate Document when appropriate
    // 12/11/2001	M Ashworth	133483
    // Added cab to determine if document previously created.
    // 03/05/2002	M Ramirez	139864
    // Added support for ARMODDEN
    // 03/06/2002	M Ramirez	139860
    // Added support for ARNOTHR
    // 10/23/2008	J Huss		CQ# 399
    // Added check for AR being the policy holder when the INSUINFO document is 
    // sent.
    // ----------------------------------------------------
    UseCabSetMaximumDiscontinueDate();

    // ----------------------------------------------------
    // If infrastructure reference date is > NULL date use
    // it as the current date.  Else, use the system date
    // ----------------------------------------------------
    if (Lt(local.Current.Date, import.Infrastructure.ReferenceDate))
    {
      local.Current.Date = import.Infrastructure.ReferenceDate;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    local.Infrastructure.ReferenceDate = local.Current.Date;

    // ----------------------------------------------------
    // If this CAB is called for a document other than the
    // ones listed below, just create a regular document trigger
    // ----------------------------------------------------
    local.IntReq.Index = -1;

    if (Equal(import.Document.Name, "ARCLOS60") || Equal
      (import.Document.Name, "ARFVLTR") || Equal
      (import.Document.Name, "CASETXFR") || Equal
      (import.Document.Name, "CHFVLTR") || Equal
      (import.Document.Name, "INSUINFO") || Equal
      (import.Document.Name, "PRMODLTR"))
    {
      if (!IsEmpty(import.SpDocKey.KeyCase))
      {
        if (ReadCase3())
        {
          MoveCase1(entities.Case1, local.Case1);
        }
      }
      else if (!IsEmpty(import.SpDocKey.KeyChild))
      {
        // ----------------------------------------------------
        // Find Case from Child
        // ----------------------------------------------------
        if (ReadCase2())
        {
          MoveCase1(entities.Case1, local.Case1);
        }
      }
      else
      {
      }

      // JHuss	10/23/2008	Do not send the letter if the AR is the insurance 
      // policy holder.
      if (Equal(import.Document.Name, "INSUINFO"))
      {
        if (ReadCaseRoleCsePerson())
        {
          if (ReadHealthInsuranceCoverage())
          {
            return;
          }
        }
      }

      // ----------------------------------------------------
      // Find the Incoming Interstate Involvement for the Case
      // ----------------------------------------------------
      foreach(var item in ReadInterstateRequest6())
      {
        local.InterstateInvolvement.Flag = "Y";

        ++local.IntReq.Index;
        local.IntReq.CheckSize();

        local.IntReq.Update.G.IntHGeneratedId =
          entities.InterstateRequest.IntHGeneratedId;

        if (local.IntReq.Index + 1 >= Local.IntReqGroup.Capacity)
        {
          break;
        }
      }

      if (AsChar(local.InterstateInvolvement.Flag) == 'Y' && AsChar
        (local.Case1.DuplicateCaseIndicator) == 'Y')
      {
        local.InterstateInvolvement.Flag = "D";
      }
    }
    else if (Equal(import.Document.Name, "NOTCCLOS"))
    {
      // ----------------------------------------------------
      // Dont send notice if interstate case closed within last 7 days. KCole
      // ----------------------------------------------------
      local.LastWeek.Date = AddDays(local.Current.Date, -7);

      if (ReadInterstateRequest1())
      {
        local.InterstateInvolvement.Flag = "Y";
      }
    }
    else if (Equal(import.Document.Name, "EMANCIPA"))
    {
      local.LiteralKansas.State = 20;

      // ----------------------------------------------------
      // Determine which document to send, if any
      // A - Do not send a document if the only order is an
      // out of state order, or the only orders are Foreign
      // Registration Orders (REGENFO, REGMODO, REGENFNJ,
      // REGMODNJ)
      // B - Send the interstate version if (A) is not true
      // and there is incoming interstate involvement
      // C - Send the Kansas version if (A) and (B) are not
      // true.
      // ----------------------------------------------------
      foreach(var item in ReadLegalAction())
      {
        if (ReadFips())
        {
          if (!Equal(entities.LegalAction.ActionTaken, "REGENFO") && !
            Equal(entities.LegalAction.ActionTaken, "REGMODO") && !
            Equal(entities.LegalAction.ActionTaken, "REGENFNJ") && !
            Equal(entities.LegalAction.ActionTaken, "REGMODNJ"))
          {
            // ----------------------------------------------------
            // A Kansas Order was found.  This means we will be
            // sending a document, but we still need to determine
            // which version
            // ----------------------------------------------------
            local.KansasOrder.Flag = "Y";

            break;
          }
        }

        local.KansasOrder.Flag = "N";
      }

      if (AsChar(local.KansasOrder.Flag) == 'N')
      {
        // ----------------------------------------------------
        // Do not send a document if the only order is an
        // out of state order, or the only orders are Foreign
        // Registration Orders (REGENFO, REGMODO, REGENFNJ,
        // REGMODNJ)
        // ----------------------------------------------------
        return;
      }

      // ----------------------------------------------------
      // Either a Kansas Order was found or no orders were found.
      // This means we will be sending a document, but we still
      // need to determine which version
      // ----------------------------------------------------
      foreach(var item in ReadInterstateRequest5())
      {
        local.InterstateInvolvement.Flag = "Y";

        ++local.IntReq.Index;
        local.IntReq.CheckSize();

        local.IntReq.Update.G.IntHGeneratedId =
          entities.InterstateRequest.IntHGeneratedId;

        if (local.IntReq.Index + 1 >= Local.IntReqGroup.Capacity)
        {
          break;
        }
      }

      if (AsChar(local.InterstateInvolvement.Flag) == 'Y')
      {
        if (ReadCase1())
        {
          local.InterstateInvolvement.Flag = "D";
        }
      }
    }
    else if (Equal(import.Document.Name, "ARLEGREQ"))
    {
      if (ReadCase3())
      {
        MoveCase1(entities.Case1, local.Case1);
      }

      // ----------------------------------------------------
      // Find the Incoming Interstate Involvement for the Case
      // ----------------------------------------------------
      foreach(var item in ReadInterstateRequest4())
      {
        local.InterstateInvolvement.Flag = "Y";

        ++local.IntReq.Index;
        local.IntReq.CheckSize();

        local.IntReq.Update.G.IntHGeneratedId =
          entities.InterstateRequest.IntHGeneratedId;

        if (local.IntReq.Index + 1 >= Local.IntReqGroup.Capacity)
        {
          break;
        }
      }

      if (AsChar(local.InterstateInvolvement.Flag) == 'Y' && AsChar
        (local.Case1.DuplicateCaseIndicator) == 'Y')
      {
        local.InterstateInvolvement.Flag = "D";
      }
    }
    else if (Equal(import.Document.Name, "ARMODDEN"))
    {
      if (ReadCase3())
      {
        MoveCase1(entities.Case1, local.Case1);
      }
      else
      {
        goto Test1;
      }

      // --------------------------------------------------------------
      // Determine which state has an active accruing obligation and
      // send them the document (send the interstate version)
      // If there is:
      // 1)  no incoming interstate involvement, or
      // 2)  no other state has an active accruing obligation, or
      // 3)  there is incoming interstate involvement related to an
      // active accruing obligation and it is a duplicate case, then
      // the AR will get the letter (send the non-interstate version)
      // --------------------------------------------------------------
      if (!ReadCsePersonAccount2())
      {
        goto Test1;
      }

      // --------------------------------------------------------------
      // An active accruing obligation must be tied to an active
      // incoming interstate case
      // --------------------------------------------------------------
      foreach(var item in ReadInterstateRequest3())
      {
        // --------------------------------------------------------------
        // An active accruing obligation is defined by active
        // accrual instructions, payable by the obligor/AP
        // --------------------------------------------------------------
        foreach(var item1 in ReadObligationTransaction())
        {
          // --------------------------------------------------------------
          // Also, make sure the obligation is in support of at least
          // one of the people on our CSE Case
          // --------------------------------------------------------------
          if (ReadCsePersonAccount1())
          {
            local.InterstateInvolvement.Flag = "Y";

            ++local.IntReq.Index;
            local.IntReq.CheckSize();

            local.IntReq.Update.G.IntHGeneratedId =
              entities.InterstateRequest.IntHGeneratedId;

            goto ReadEach;
          }
        }
      }

ReadEach:
      ;
    }
    else if (Equal(import.Document.Name, "ARNOTHR"))
    {
      if (import.SpDocKey.KeyLegalAction > 0)
      {
        foreach(var item in ReadCaseCaseRole())
        {
          if (ReadInterstateRequest2())
          {
            local.InterstateInvolvement.Flag = "Y";

            break;
          }
        }
      }
    }
    else
    {
    }

Test1:

    // --------------------------------------------------------------
    // Interstate Involvement is Incoming or may also be a Duplicate Case.
    // Send the interstate version
    // --------------------------------------------------------------
    if (!IsEmpty(local.InterstateInvolvement.Flag))
    {
      // ----------------------------------------------------
      // Send the interstate version
      // Send one document for each interstate request.  If
      // the Case number had been imported, then send
      // documents for that case only
      // NOTE:  For some documents, it will not be sent if
      // there is interstate involvement
      // ----------------------------------------------------
      switch(TrimEnd(import.Document.Name))
      {
        case "ARLEGREQ":
          local.Document.Name = "ISLEGREQ";

          break;
        case "ARMODDEN":
          local.Document.Name = "ISARMODD";

          break;
        case "EMANCIPA":
          local.Document.Name = "ISEMANCI";

          break;
        case "INSUINFO":
          local.Document.Name = "ISINSUIN";

          break;
        default:
          goto Test2;
      }

      if (!IsEmpty(import.Document.Type1))
      {
        local.Document.Type1 = import.Document.Type1;
      }

      local.SpDocKey.Assign(import.SpDocKey);
      local.IntReq.Index = 0;

      for(var limit = local.IntReq.Count; local.IntReq.Index < limit; ++
        local.IntReq.Index)
      {
        if (!local.IntReq.CheckSize())
        {
          break;
        }

        local.SpDocKey.KeyInterstateRequest =
          local.IntReq.Item.G.IntHGeneratedId;

        if (AsChar(import.FindDoc.Flag) == 'Y')
        {
          // mca
          // ----------------------------------------------------
          // 12-11-01
          // check if document already created
          // ----------------------------------------------------
          UseSpDocFindOutgoingDocument2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          if (Equal(import.Document.Name, "EMANCIPA"))
          {
            if (!IsEmpty(local.OutgoingDocument.PrintSucessfulIndicator))
            {
              if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'N')
              {
                // mca
                // ----------------------------------------------------
                // 12-11-01
                // Document for this child was unsuccessfully printed.
                // Try it again using same outgoing_doc.
                // ----------------------------------------------------
              }
              else
              {
                ++export.ControlDocsPrevPrints.Count;

                continue;
              }
            }
          }
          else
          {
            switch(AsChar(local.OutgoingDocument.PrintSucessfulIndicator))
            {
              case 'Y':
                // Successfully printed already.
                // Update indicates new data, create a new outgoing_doc.
                local.Infrastructure.SystemGeneratedIdentifier = 0;

                break;
              case 'N':
                // Unsuccessful printing.
                // Use the old outgoing_doc.
                ++export.ControlDocsPrevPrints.Count;

                break;
              case 'G':
                // Document is not yet printed and data is not yet retrieved.
                // Use the old outgoing_doc
                ++export.ControlDocsPrevPrints.Count;

                break;
              case 'C':
                // Canceled.  Create a new outgoing doc.
                local.Infrastructure.SystemGeneratedIdentifier = 0;

                break;
              case ' ':
                // No previous printing found.  Create a new outgoing_doc.
                local.Infrastructure.SystemGeneratedIdentifier = 0;

                break;
              default:
                break;
            }
          }
        }

        UseSpCreateDocumentInfrastruct2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ++export.InterstateDocsCreated.Count;
      }

      local.IntReq.CheckIndex();
    }

Test2:

    // --------------------------------------------------------------
    // Interstate Involvement is Outgoing or Non-existant or it is a Duplicate 
    // Case.
    // Send the regular (non-interstate) version
    // --------------------------------------------------------------
    if (AsChar(local.InterstateInvolvement.Flag) <= 'D')
    {
      // mca
      // ----------------------------------------------------
      // 12-11-01
      // check if document already created
      // ----------------------------------------------------
      if (AsChar(import.FindDoc.Flag) == 'Y')
      {
        UseSpDocFindOutgoingDocument1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(import.Document.Name, "EMANCIPA"))
        {
          if (!IsEmpty(local.OutgoingDocument.PrintSucessfulIndicator))
          {
            if (AsChar(local.OutgoingDocument.PrintSucessfulIndicator) == 'N')
            {
              // mca
              // ----------------------------------------------------
              // 12-11-01
              // Document for this child was unsuccessfully printed.
              // Try it again using same outgoing_doc.
              // ----------------------------------------------------
            }
            else
            {
              ++export.ControlDocsPrevPrints.Count;

              return;
            }
          }
        }
        else
        {
          switch(AsChar(local.OutgoingDocument.PrintSucessfulIndicator))
          {
            case 'Y':
              // Successfully printed already.
              // Update indicates new data, create a new outgoing_doc.
              local.Infrastructure.SystemGeneratedIdentifier = 0;

              break;
            case 'N':
              // Unsuccessful printing.
              // Use the old outgoing_doc.
              ++export.ControlDocsPrevPrints.Count;

              break;
            case 'G':
              // Document is not yet printed and data is not yet retrieved.
              // Use the old outgoing_doc
              ++export.ControlDocsPrevPrints.Count;

              break;
            case 'C':
              // Canceled.  Create a new outgoing doc.
              local.Infrastructure.SystemGeneratedIdentifier = 0;

              break;
            case ' ':
              // No previous printing found.  Create a new outgoing_doc.
              local.Infrastructure.SystemGeneratedIdentifier = 0;

              break;
            default:
              return;
          }
        }
      }

      UseSpCreateDocumentInfrastruct1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.KansasDocsCreated.Count = 1;
      }
    }
  }

  private static void MoveCase1(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.DuplicateCaseIndicator = source.DuplicateCaseIndicator;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Type1 = source.Type1;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminAction = source.KeyAdminAction;
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyAdminAppeal = source.KeyAdminAppeal;
    target.KeyAp = source.KeyAp;
    target.KeyAppointment = source.KeyAppointment;
    target.KeyAr = source.KeyAr;
    target.KeyBankruptcy = source.KeyBankruptcy;
    target.KeyCase = source.KeyCase;
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyChild = source.KeyChild;
    target.KeyContact = source.KeyContact;
    target.KeyGeneticTest = source.KeyGeneticTest;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyIncarceration = source.KeyIncarceration;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyInfoRequest = source.KeyInfoRequest;
    target.KeyInterstateRequest = source.KeyInterstateRequest;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyLegalActionDetail = source.KeyLegalActionDetail;
    target.KeyLegalReferral = source.KeyLegalReferral;
    target.KeyLocateRequestAgency = source.KeyLocateRequestAgency;
    target.KeyLocateRequestSource = source.KeyLocateRequestSource;
    target.KeyMilitaryService = source.KeyMilitaryService;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationAdminAction = source.KeyObligationAdminAction;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
    target.KeyResource = source.KeyResource;
    target.KeyTribunal = source.KeyTribunal;
    target.KeyWorkerComp = source.KeyWorkerComp;
    target.KeyWorksheet = source.KeyWorksheet;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSpCreateDocumentInfrastruct1()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveSpDocKey(import.SpDocKey, useImport.SpDocKey);
    MoveDocument(import.Document, useImport.Document);
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct2()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    MoveDocument(local.Document, useImport.Document);
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void UseSpDocFindOutgoingDocument1()
  {
    var useImport = new SpDocFindOutgoingDocument.Import();
    var useExport = new SpDocFindOutgoingDocument.Export();

    useImport.SpDocKey.Assign(import.SpDocKey);
    MoveDocument(import.Document, useImport.Document);

    Call(SpDocFindOutgoingDocument.Execute, useImport, useExport);

    local.OutgoingDocument.PrintSucessfulIndicator =
      useExport.OutgoingDocument.PrintSucessfulIndicator;
    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private void UseSpDocFindOutgoingDocument2()
  {
    var useImport = new SpDocFindOutgoingDocument.Import();
    var useExport = new SpDocFindOutgoingDocument.Export();

    useImport.SpDocKey.Assign(local.SpDocKey);
    MoveDocument(local.Document, useImport.Document);

    Call(SpDocFindOutgoingDocument.Execute, useImport, useExport);

    local.OutgoingDocument.PrintSucessfulIndicator =
      useExport.OutgoingDocument.PrintSucessfulIndicator;
    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase3()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseRole()
  {
    entities.Ap.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", import.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Ap.CasNumber = db.GetString(reader, 0);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 1);
        entities.Ap.CspNumber = db.GetString(reader, 2);
        entities.Ap.Type1 = db.GetString(reader, 3);
        entities.Ap.Identifier = db.GetInt32(reader, 4);
        entities.Ap.StartDate = db.GetNullableDate(reader, 5);
        entities.Ap.EndDate = db.GetNullableDate(reader, 6);
        entities.Ap.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ap.Type1);

        return true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", local.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePersonAccount1()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Supported.Populated = false;

    return Read("ReadCsePersonAccount1",
      (db, command) =>
      {
        db.SetString(
          command, "type", entities.ObligationTransaction.CpaSupType ?? "");
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspSupNumber ?? ""
          );
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);
      });
  }

  private bool ReadCsePersonAccount2()
  {
    entities.Obligor.Populated = false;

    return Read("ReadCsePersonAccount2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.SpDocKey.KeyAp);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
        db.SetInt32(command, "state", local.LiteralKansas.State);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.
          SetInt64(command, "identifier", import.SpDocKey.KeyHealthInsCoverage);
          
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "othStateClsDte", local.LastWeek.Date.GetValueOrDefault());
        db.SetNullableString(command, "casINumber", import.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 8);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.Ap.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.Ap.Identifier);
        db.SetNullableString(command, "croType", entities.Ap.Type1);
        db.SetNullableString(command, "cspNumber", entities.Ap.CspNumber);
        db.SetNullableString(command, "casNumber", entities.Ap.CasNumber);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 8);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest3()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest3",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", local.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.SpDocKey.KeyAp);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 8);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest4()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest4",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", local.Case1.Number);
        db.SetInt32(command, "lgrId", import.SpDocKey.KeyLegalReferral);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 8);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest5()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest5",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 8);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest6()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest6",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 8);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
        db.SetDate(command, "asOfDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of FindDoc.
    /// </summary>
    [JsonPropertyName("findDoc")]
    public Common FindDoc
    {
      get => findDoc ??= new();
      set => findDoc = value;
    }

    private Infrastructure infrastructure;
    private SpDocKey spDocKey;
    private Document document;
    private Common findDoc;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateDocsCreated.
    /// </summary>
    [JsonPropertyName("interstateDocsCreated")]
    public Common InterstateDocsCreated
    {
      get => interstateDocsCreated ??= new();
      set => interstateDocsCreated = value;
    }

    /// <summary>
    /// A value of KansasDocsCreated.
    /// </summary>
    [JsonPropertyName("kansasDocsCreated")]
    public Common KansasDocsCreated
    {
      get => kansasDocsCreated ??= new();
      set => kansasDocsCreated = value;
    }

    /// <summary>
    /// A value of ControlDocsPrevPrints.
    /// </summary>
    [JsonPropertyName("controlDocsPrevPrints")]
    public Common ControlDocsPrevPrints
    {
      get => controlDocsPrevPrints ??= new();
      set => controlDocsPrevPrints = value;
    }

    private Common interstateDocsCreated;
    private Common kansasDocsCreated;
    private Common controlDocsPrevPrints;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A IntReqGroup group.</summary>
    [Serializable]
    public class IntReqGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateRequest G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private InterstateRequest g;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of LastWeek.
    /// </summary>
    [JsonPropertyName("lastWeek")]
    public DateWorkArea LastWeek
    {
      get => lastWeek ??= new();
      set => lastWeek = value;
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of KansasOrder.
    /// </summary>
    [JsonPropertyName("kansasOrder")]
    public Common KansasOrder
    {
      get => kansasOrder ??= new();
      set => kansasOrder = value;
    }

    /// <summary>
    /// A value of LiteralKansas.
    /// </summary>
    [JsonPropertyName("literalKansas")]
    public Fips LiteralKansas
    {
      get => literalKansas ??= new();
      set => literalKansas = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// Gets a value of IntReq.
    /// </summary>
    [JsonIgnore]
    public Array<IntReqGroup> IntReq => intReq ??= new(IntReqGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of IntReq for json serialization.
    /// </summary>
    [JsonPropertyName("intReq")]
    [Computed]
    public IList<IntReqGroup> IntReq_Json
    {
      get => intReq;
      set => IntReq.Assign(value);
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of InterstateInvolvement.
    /// </summary>
    [JsonPropertyName("interstateInvolvement")]
    public Common InterstateInvolvement
    {
      get => interstateInvolvement ??= new();
      set => interstateInvolvement = value;
    }

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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private CsePerson ar;
    private DateWorkArea lastWeek;
    private DateWorkArea max;
    private OutgoingDocument outgoingDocument;
    private Case1 case1;
    private DateWorkArea null1;
    private Common kansasOrder;
    private Fips literalKansas;
    private SpDocKey spDocKey;
    private Array<IntReqGroup> intReq;
    private Document document;
    private Common interstateInvolvement;
    private DateWorkArea current;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of LegalReferralCaseRole.
    /// </summary>
    [JsonPropertyName("legalReferralCaseRole")]
    public LegalReferralCaseRole LegalReferralCaseRole
    {
      get => legalReferralCaseRole ??= new();
      set => legalReferralCaseRole = value;
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

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private HealthInsuranceCoverage healthInsuranceCoverage;
    private InterstateRequestObligation interstateRequestObligation;
    private ObligationType obligationType;
    private CaseRole ap;
    private CsePersonAccount supported;
    private CsePersonAccount obligor;
    private AccrualInstructions accrualInstructions;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private LegalReferralCaseRole legalReferralCaseRole;
    private LegalReferral legalReferral;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private Fips fips;
    private Tribunal tribunal;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction legalAction;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
  }
#endregion
}
