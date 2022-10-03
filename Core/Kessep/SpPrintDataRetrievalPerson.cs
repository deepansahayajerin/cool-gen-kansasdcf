// Program: SP_PRINT_DATA_RETRIEVAL_PERSON, ID: 372132895, model: 746.
// Short name: SWE02234
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_PRINT_DATA_RETRIEVAL_PERSON.
/// </summary>
[Serializable]
public partial class SpPrintDataRetrievalPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_PRINT_DATA_RETRIEVAL_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpPrintDataRetrievalPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpPrintDataRetrievalPerson.
  /// </summary>
  public SpPrintDataRetrievalPerson(IContext context, Import import,
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
    // ------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ------------------------------------------------------------------------------------------
    // 11/17/1998	M Ramirez			Initial Development
    // 03/19/1999	M Ramirez			Split out sp_doc_get_person details
    // 07/14/1999	M Ramirez			Added row lock counts
    // 09/16/1999	M Ramirez	73914		Added alternate methods for LEA Children
    // 09/30/1999	M Ramirez	75829		Modifications to alternate methods
    // 11/29/1999	D Lowry		79759		Find employer HQ if specified
    // 03/01/2000	M Ramirez	81578		Business rule change for APEMP_LA3
    // 03/16/2000	M Ramirez	WR 162		Added Family Violence fields
    // 07/14/2000	M Ramirez	WR 162		Added alternate methods for 'CAS' business 
    // object
    // 10/03/2000	M Ramirez	102858		Changed APEMP_LA2 and APEMP_LA3 to account 
    // for Case
    // 						Number if it is given.
    // 01/08/2001	M Ramirez	110446		Fixed code so that sp_doc_get_person would 
    // only
    // 						be called once per Case Role type
    // 01/08/2001	M Ramirez	none		Marked export_error_ind zdel
    // 03/14/2001	M Ramirez	WR 187		Added APEMPEIN
    // 				Seg G
    // 05/08/2001	M Ramirez	WR291		Added *LOCADDR1-5 for locate request
    // 9/15/2001	M Ramirez	124181		Fixed height and weight fields
    // 09/18/2001	M Ramirez	124677		Fixed employer fields for when an AP and AR 
    // employer is included
    // 02/07/2002	M Ramirez	PR120947	If AP is not found using alternate method, 
    // then use default method
    // 03/07/2002	G Vandy		PR123074	Process resident agent address as an address
    // group
    // 03/20/2002	K Cole		PR127439	Changed so that if there is no contact name 
    // on Other Policy
    // 						Holder, contact company name will populate APNM field.
    // 10/03/2002	K Cole		WR20313		Added foreign country POB - will download 
    // with country
    // 						name spelled out. Also changed so state name will be spelled out
    // 06/30/2003	GVandy		PR176328	If income source id is not populated default 
    // to most recent
    // 						non end dated income source.
    // 03/24/2006	M J Quinn	PR266315	When printing from GTSC screen, only print 
    // attorney information for the specified case.
    // Use Last Tran to identify the GTSC screen.
    // 08/10/2007	G. Pan		PR310519	Commented out the logic that was inside of 
    // CASE OF field subroutine_name = "INCMSRCE" .
    // the codes was prevented ownload/printing the document from DDOC PF24 when
    // the Family violence is set on COMN screen.
    // 03/04/2008	M Ramirez	WR 276, 277	Added fields for masked SSNs  (*SSNXX, *
    // *SSNXX, *MOSSNXX, **MOSSNXX)
    // 04/11/2008	M Ramirez	WR 302941	Added fields for DET2PATP (Multiple APs 
    // Home Phone, Sex, SSN, DOB)
    // 04/21/2008	M Ramirez	WR 302941	Added field for Family Violence for 
    // Multiple APs
    // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and DOB 
    // placeholders.
    // 05/13/2009	J Huss		CQ# 11072	Corrected view being passed to 
    // sp_doc_format_ssn.
    // 02/24/2010	J Huss		CQ# 15403	APEMP LA3 was not used anywhere in system.  
    // Removed logic for field
    // 						and replaced with same logic as APEMP LA2 - except ignoring start
    // 						and end dates for AP.
    // 03/18/2010	J Huss		CQ# 16237	Added check to *MO address fields to account
    // for FVI.
    // 12/06/2010	J Huss		CQ# 15776	Added support for FA fields.
    // ------------------------------------------------------------------------------------------
    // **************************************************************************************
    // *                               
    // Maintenance Log
    // 
    // *
    // **************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // **************************************************************************************
    // * 06/15/2011  Raj S              CQ15776     Modified to fix MA/FA Name 
    // and Address  *
    // *
    // 
    // issues and re-click the code change due *
    // *
    // 
    // NPI back out.
    // *
    // *
    // **************************************************************************************
    // ------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ------------------------------------------------------------------------------------------
    // 09-16-2015  GVandy		CQ22212		a) E-IWO requires Employer fields to be 
    // retrieved
    // 						   when FV is set for the AP.
    // 						b) Add first name, middle intitial, and last name
    // 						   fields for children.
    // 01-09-2017  GVandy		CQ41786		Add support for WL legal details for new
    // 						ORDIWOLS document.
    // 11-21-2018  GVandy		CQ61457		SSA changes.
    // 04-11-2021  GVandy		CQ68567 	Add support for CH0 ZCS field.
    // ------------------------------------------------------------------------------------------
    MoveFieldValue2(import.FieldValue, local.FieldValue);
    export.SpDocKey.Assign(import.SpDocKey);
    local.ChAlternateMethods.Index = -1;
    local.ApAlternateMethods.Index = -1;

    // mjr
    // --------------------------------------------------------------------------------
    // 03/04/2008
    // WR 276, 277	Added fields for masked SSNs  (*SSNXX, **SSNXX, *MOSSNXX, **
    // MOSSNXX)
    // The flag (Local Mask SSN) is only passed to SP_DOC_FORMAT_SSN when 
    // masking is required.
    // ---------------------------------------------------------------------------------------------
    local.MaskSsn.Flag = "Y";

    // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and DOB 
    // placeholders.
    local.PopulatePlaceholder.Flag = "Y";

    // mjr
    // -----------------------------------------------
    // 10/06/2001
    // Changed sort order from dependancy, subroutine, name
    // to name, subroutine
    // ------------------------------------------------------------
    foreach(var item in ReadField())
    {
      if (!Lt(local.PreviousField.Name, entities.Field.Name))
      {
        continue;
      }

      local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      local.ProcessGroup.Flag = "N";
      local.SpPrintWorkSet.Assign(local.NullSpPrintWorkSet);
      local.PersonPrivateAttorney.Assign(local.NullPersonPrivateAttorney);
      MoveField(entities.Field, local.PreviousField);
      local.CurrentCaseRole.Type1 = Substring(entities.Field.Name, 1, 2);

      if (!IsEmpty(local.SkipAp.Flag))
      {
        if (Equal(local.CurrentCaseRole.Type1, "AP"))
        {
          local.PreviousField.Name = local.CurrentCaseRole.Type1 + "9";

          continue;
        }
      }

      if (!IsEmpty(local.SkipCh.Flag))
      {
        if (Equal(local.CurrentCaseRole.Type1, "CH"))
        {
          local.PreviousField.Name = local.CurrentCaseRole.Type1 + "9";

          continue;
        }
      }

      local.CurrentNumberField.Name = Substring(entities.Field.Name, 3, 1);

      if (!Lt(local.CurrentNumberField.Name, "0") && !
        Lt("9", local.CurrentNumberField.Name))
      {
        // mjr
        // ----------------------------------------------------------------------
        // This is a group of people that fulfill a specific role for a
        // specific business object.
        // Determine the generic name for this field.
        // The person number is the number between the role code (ie "CH") and 
        // the
        // last 2-3 characters of the field.
        // The generic name is the original field name, with the
        // person number replaced by a '*', (ie CH*NM).
        // -------------------------------------------------------------------------
        local.CurrentNumberCommon.Count =
          (int)StringToNumber(Substring(
            entities.Field.Name, Field.Name_MaxLength, 3, 1));
        local.CurrentField.Name = "**" + Substring
          (entities.Field.Name, Field.Name_MaxLength, 4, 7);

        switch(TrimEnd(local.CurrentCaseRole.Type1))
        {
          case "AP":
            if (!Equal(local.CurrentCaseRole.Type1, local.PreviousCaseRole.Type1))
              
            {
              local.NeedGroup.Flag = "Y";
              UseSpDocGetPerson3();

              // mjr
              // ---------------------------------------------------
              // 01/08/2001
              // PR# 110446 - Only set this if the call to sp_doc_get_person
              // has been made
              // ----------------------------------------------------------------
              local.PreviousCaseRole.Type1 = local.CurrentCaseRole.Type1;

              if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
                IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsEmpty(export.ErrorDocumentField.ScreenPrompt))
                {
                  export.ErrorDocumentField.ScreenPrompt = "Processing Error";
                  export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                    (entities.Field.Name) + ",  Subroutine:  " + entities
                    .Field.SubroutineName;
                }

                return;
              }

              if (local.Ap.Count <= 0)
              {
                local.CsePersonsWorkSet.Number = "";
                local.SkipAp.Flag = "Y";

                break;
              }
            }

            if (local.Ap.Count < local.CurrentNumberCommon.Count + 1)
            {
              local.CsePersonsWorkSet.Number = "";
              local.SkipAp.Flag = "Y";
            }
            else
            {
              local.Ap.Index = local.CurrentNumberCommon.Count;
              local.Ap.CheckSize();

              local.CsePerson.Assign(local.Ap.Item.GlocalApCsePerson);
              MoveCsePersonsWorkSet2(local.Ap.Item.GlocalApCsePersonsWorkSet,
                local.CsePersonsWorkSet);
            }

            break;
          case "CH":
            if (!Equal(local.CurrentCaseRole.Type1, local.PreviousCaseRole.Type1))
              
            {
              if (Equal(import.Document.BusinessObject, "LEA") || Equal
                (import.Document.BusinessObject, "NOA") || Equal
                (import.Document.BusinessObject, "CAS"))
              {
                local.ObligationType.Code =
                  Substring(entities.Field.Name, 5, 6);

                // --04-11-2021 GVandy CQ68567 Add support for CH0 ZCS field.
                if (Equal(local.ObligationType.Code, "AJ") || Equal
                  (local.ObligationType.Code, "CRCH") || Equal
                  (local.ObligationType.Code, "CS") || Equal
                  (local.ObligationType.Code, "EP") || Equal
                  (local.ObligationType.Code, "FEE") || Equal
                  (local.ObligationType.Code, "HIC") || Equal
                  (local.ObligationType.Code, "MJ") || Equal
                  (local.ObligationType.Code, "WA") || Equal
                  (local.ObligationType.Code, "WC") || Equal
                  (local.ObligationType.Code, "WL") || Equal
                  (local.ObligationType.Code, "718B") || Equal
                  (local.ObligationType.Code, "ZCS"))
                {
                  if (Equal(import.Document.BusinessObject, "LEA") || Equal
                    (import.Document.BusinessObject, "NOA"))
                  {
                    ++local.ChAlternateMethods.Index;
                    local.ChAlternateMethods.CheckSize();

                    local.ChAlternateMethods.Update.GlocalCh.Code =
                      local.ObligationType.Code;
                  }

                  local.CurrentField.Name = "** LOPS";

                  break;
                }
                else if (Equal(local.ObligationType.Code, "IGNEND"))
                {
                  ++local.ChAlternateMethods.Index;
                  local.ChAlternateMethods.CheckSize();

                  local.ChAlternateMethods.Update.GlocalCh.Code =
                    local.ObligationType.Code;
                  local.CurrentField.Name = "** LROL";

                  break;
                }
                else if (Equal(local.ObligationType.Code, "FV"))
                {
                  if (Equal(import.Document.BusinessObject, "CAS"))
                  {
                    ++local.ChAlternateMethods.Index;
                    local.ChAlternateMethods.CheckSize();

                    local.ChAlternateMethods.Update.GlocalCh.Code =
                      local.ObligationType.Code;
                  }
                  else
                  {
                    local.DetermineChFv.Name = entities.Field.Name;
                  }

                  local.CurrentField.Name = "** FV";

                  break;
                }
                else
                {
                }
              }

              local.NeedGroup.Flag = "Y";
              UseSpDocGetPerson2();

              // mjr
              // ---------------------------------------------------
              // 01/08/2001
              // PR# 110446 - Only set this if the call to sp_doc_get_person
              // has been made
              // ----------------------------------------------------------------
              local.PreviousCaseRole.Type1 = local.CurrentCaseRole.Type1;

              if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
                IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsEmpty(export.ErrorDocumentField.ScreenPrompt))
                {
                  export.ErrorDocumentField.ScreenPrompt = "Processing Error";
                  export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                    (entities.Field.Name) + ",  Subroutine:  " + entities
                    .Field.SubroutineName;
                }

                return;
              }

              // mjr
              // ------------------------------------------
              // 10/19/2000
              // PR# 102858 - Added so alternate methods group could
              // be reused
              // -------------------------------------------------------
              local.ChAlternateMethods.Count = 0;

              if (local.Ch.Count <= 0)
              {
                local.CsePersonsWorkSet.Number = "";
                local.SkipCh.Flag = "Y";

                break;
              }
            }

            if (local.Ch.Count < local.CurrentNumberCommon.Count + 1)
            {
              local.CsePersonsWorkSet.Number = "";
              local.SkipCh.Flag = "Y";
            }
            else
            {
              local.Ch.Index = local.CurrentNumberCommon.Count;
              local.Ch.CheckSize();

              local.CsePerson.Assign(local.Ch.Item.GlocalChCsePerson);
              MoveCsePersonsWorkSet2(local.Ch.Item.GlocalChCsePersonsWorkSet,
                local.CsePersonsWorkSet);
            }

            break;
          default:
            break;
        }

        // mjr
        // ---------------------------------------------------
        // 09/18/2001
        // Reset this to get information on a new income source
        // ----------------------------------------------------------------
        local.IncomeSource.Identifier = local.NullDateWorkArea.Timestamp;
      }
      else
      {
        local.CurrentField.Name = "*" + Substring
          (entities.Field.Name, Field.Name_MaxLength, 3, 8);

        // mjr
        // -------------------------------------------------------------
        // Determine person number based on the Business Object Code.
        // (Also, may use a special situation if a special field is
        // included on the document)
        // ----------------------------------------------------------------
        if (Equal(local.CurrentCaseRole.Type1, local.PreviousCaseRole.Type1))
        {
          // mjr
          // ---------------------------------------------------
          // 01/08/2001
          // PR# 110446 - Sp_doc_get_person has already been called for this 
          // Case Role type.  Don't call it again.
          // ----------------------------------------------------------------
          goto Test1;
        }

        // mjr
        // ---------------------------------------------------
        // 10/11/2001
        // CH FV will be calculated after all the children are found
        // (in the case of multiple children requested for the document)
        // This will allow CH FV to be used for one specific child
        // and multiple children.
        // ----------------------------------------------------------------
        if (Equal(entities.Field.Name, "CH FV"))
        {
          local.DetermineChFv.Name = entities.Field.Name;

          goto Test1;
        }

        if (Equal(import.Document.BusinessObject, "LEA") || Equal
          (import.Document.BusinessObject, "NOA"))
        {
          switch(TrimEnd(local.CurrentCaseRole.Type1))
          {
            case "AP":
              // mjr
              // ---------------------------------------------------------
              // Methods for finding an AP on a Legal Action
              // AP LA1  --- default, if no other method is found use this one;
              // AP LA2  --- included on documents IWOMODO, IWOTERM and MWO;
              // AP LA3  --- included on documents EMPMWOJ, TERMMWOM and 
              // TERMMWOO;
              // AP LA4  --- included on documents GARNO and GARNAFFT;
              // AP LA5  --- included on document EMPIWOJ
              // ------------------------------------------------------------
              // mjr
              // ------------------------------------------
              // 10/19/2000
              // PR# 102858 - Added check for '* FV' so family violence
              // would only factor in after the other alternate methods
              // -------------------------------------------------------
              if (Equal(local.CurrentField.Name, "* FV"))
              {
                local.ApAlternateMethods.Index = 0;
                local.ApAlternateMethods.CheckSize();

                local.ApAlternateMethods.Update.GlocalAp.Code = "FV";

                continue;
              }
              else if (Equal(local.CurrentField.Name, "* LA2") || Equal
                (local.CurrentField.Name, "* LA3") || Equal
                (local.CurrentField.Name, "* LA4") || Equal
                (local.CurrentField.Name, "* LA5") || Equal
                (local.CurrentField.Name, "* LA6"))
              {
                local.AlternateMethod.Flag =
                  Substring(entities.Field.Name, 6, 1);
              }
              else
              {
                local.AlternateMethod.Flag = "";
              }

              break;
            case "AR":
              local.AlternateMethod.Flag = "";

              break;
            case "CH":
              local.AlternateMethod.Flag = "";

              break;
            case "PR":
              local.AlternateMethod.Flag = "";

              break;
            default:
              break;
          }
        }
        else
        {
          local.AlternateMethod.Flag = "";
        }

        local.NeedGroup.Flag = "";
        UseSpDocGetPerson1();

        // mjr
        // ---------------------------------------------------
        // 02/07/2002
        // PR# 120947 - Changed so that local_previous only gets set
        // if the AP hasn't been found AND it wasn't an alternate method.
        // That way the default method will be used if all else fails.
        // ----------------------------------------------------------------
        if (!IsEmpty(local.CsePersonsWorkSet.Number))
        {
          // mjr
          // ---------------------------------------------------
          // 10/19/2000
          // PR# 102858 - Only set this if the call to sp_doc_get_person
          // has been made
          // ----------------------------------------------------------------
          local.PreviousCaseRole.Type1 = local.CurrentCaseRole.Type1;
        }
        else if (IsEmpty(local.AlternateMethod.Flag))
        {
          // mjr
          // ---------------------------------------------------
          // 10/19/2000
          // PR# 102858 - Only set this if the call to sp_doc_get_person
          // has been made
          // ----------------------------------------------------------------
          local.PreviousCaseRole.Type1 = local.CurrentCaseRole.Type1;
          local.PreviousField.Name = local.CurrentCaseRole.Type1 + "Z";
        }

        local.AlternateMethod.Flag = "";

        // mjr
        // ---------------------------------------------------
        // 09/18/2001
        // Reset this to get information on a new income source
        // ----------------------------------------------------------------
        local.IncomeSource.Identifier = local.NullDateWorkArea.Timestamp;
        local.PersonPrivateAttorney.Identifier = 0;

        if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
          IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsEmpty(export.ErrorDocumentField.ScreenPrompt))
          {
            export.ErrorDocumentField.ScreenPrompt = "Processing Error";
            export.ErrorFieldValue.Value = "Field:  " + TrimEnd
              (entities.Field.Name) + ",  Person:  " + local
              .CsePersonsWorkSet.Number;
          }

          return;
        }

        if (!IsEmpty(local.CsePersonsWorkSet.Number))
        {
          // mjr
          // ---------------------------------------------------
          // 10/19/2000
          // PR# 102858 - In the special cases for legal action, need to
          // account for the '* FV' field.
          // If the '* FV' field is on this document then check for family
          // violence on this person.  If it exists, set the field_value
          // to the family violence indicator and continue processing
          // the '* LAx' field as normal
          // Currently this requirement only exists for APs
          // ----------------------------------------------------------------
          if (Equal(local.CurrentCaseRole.Type1, "AP"))
          {
            if (local.ApAlternateMethods.Count > 0)
            {
              local.ApAlternateMethods.Index = 0;
              local.ApAlternateMethods.CheckSize();

              if (Equal(local.ApAlternateMethods.Item.GlocalAp.Code, "FV"))
              {
                if (!IsEmpty(local.CsePerson.FamilyViolenceIndicator))
                {
                  local.ApFv.FamilyViolenceIndicator =
                    local.CsePerson.FamilyViolenceIndicator ?? "";
                  local.Temp.Name = "AP FV";
                  local.FieldValue.Value =
                    local.CsePerson.FamilyViolenceIndicator ?? "";
                  UseSpCabCreateUpdateFieldValue2();

                  if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                  {
                    ExitState = "ACO_NN0000_ALL_OK";
                  }

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    export.ErrorDocumentField.ScreenPrompt = "Creation Error";
                    export.ErrorFieldValue.Value = "Field:  " + local.Temp.Name;

                    return;
                  }

                  local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
                  ++import.ExpImpRowLockFieldValue.Count;
                }

                local.ApAlternateMethods.Count = 0;
              }
            }
          }
        }
      }

Test1:

      // mjr
      // --------------------------------------
      // 10/11/2001
      // Removed ** FV from the following if statement
      // so that no value is given for the field unless
      // family violence is turned on for at least one child
      // ---------------------------------------------------
      if (IsEmpty(local.CsePersonsWorkSet.Number) && !
        Equal(local.CurrentField.Name, "** LROL") && !
        Equal(local.CurrentField.Name, "** LOPS"))
      {
        continue;
      }

      switch(TrimEnd(entities.Field.SubroutineName))
      {
        case " DETERMN":
          switch(TrimEnd(local.CurrentField.Name))
          {
            case "* FV":
              // mjr
              // ----------------------------------------------
              // 10/11/2001
              // Removed CH from this processing to alleviate confusion
              // like that is going to help
              // -----------------------------------------------------------
              if (!IsEmpty(local.CsePerson.FamilyViolenceIndicator))
              {
                local.FieldValue.Value =
                  local.CsePerson.FamilyViolenceIndicator ?? "";

                switch(TrimEnd(local.CurrentCaseRole.Type1))
                {
                  case "AP":
                    local.ApFv.FamilyViolenceIndicator =
                      local.CsePerson.FamilyViolenceIndicator ?? "";

                    break;
                  case "AR":
                    local.ArFv.FamilyViolenceIndicator =
                      local.CsePerson.FamilyViolenceIndicator ?? "";

                    break;
                  case "CH":
                    break;
                  case "PR":
                    local.PrFv.FamilyViolenceIndicator =
                      local.CsePerson.FamilyViolenceIndicator ?? "";

                    break;
                  default:
                    break;
                }
              }
              else
              {
              }

              break;
            case "* LA1":
              local.FieldValue.Value = local.CsePersonsWorkSet.Number;

              break;
            case "* LA2":
              local.FieldValue.Value = local.CsePersonsWorkSet.Number;

              break;
            case "* LA3":
              local.FieldValue.Value = local.CsePersonsWorkSet.Number;

              break;
            case "* LA4":
              local.FieldValue.Value = local.CsePersonsWorkSet.Number;

              break;
            case "* LA5":
              local.FieldValue.Value = local.CsePersonsWorkSet.Number;

              break;
            case "* LA6":
              local.FieldValue.Value = local.CsePersonsWorkSet.Number;

              break;
            case "*EMP LA1":
              if (ReadIncomeSource2())
              {
                export.SpDocKey.KeyIncomeSource =
                  entities.IncomeSource.Identifier;
                local.BatchTimestampWorkArea.IefTimestamp =
                  entities.IncomeSource.Identifier;
                UseLeCabConvertTimestamp();
                local.FieldValue.Value =
                  local.BatchTimestampWorkArea.TextTimestamp;
              }

              break;
            case "*EMP LA2":
              // mjr
              // -------------------------------------------
              // 10/03/2000
              // PR# 102858 - Account for Case Number if it is given
              // --------------------------------------------------------
              // mjr
              // -------------------------------------------
              // 03/01/2000
              // PR# 81578 - APEMP_LA3 needs to account for multiple
              // Legal_action_income_sources per Legal_action
              // --------------------------------------------------------
              if (!IsEmpty(export.SpDocKey.KeyCase))
              {
                if (ReadLegalActionIncomeSourceIncomeSource1())
                {
                  export.SpDocKey.KeyIncomeSource =
                    entities.IncomeSource.Identifier;
                  local.BatchTimestampWorkArea.IefTimestamp =
                    entities.IncomeSource.Identifier;
                  UseLeCabConvertTimestamp();
                  local.FieldValue.Value =
                    local.BatchTimestampWorkArea.TextTimestamp;
                }
              }
              else if (ReadLegalActionIncomeSourceIncomeSource3())
              {
                export.SpDocKey.KeyIncomeSource =
                  entities.IncomeSource.Identifier;
                local.BatchTimestampWorkArea.IefTimestamp =
                  entities.IncomeSource.Identifier;
                UseLeCabConvertTimestamp();
                local.FieldValue.Value =
                  local.BatchTimestampWorkArea.TextTimestamp;
              }

              break;
            case "*EMP LA3":
              // 02/24/2010	JHuss	CQ# 15403	APEMP LA3 was not used anywhere in 
              // system.  Removed logic for field
              // 					and replaced with same logic as APEMP LA2 - except 
              // ignoring start
              // 					and end dates for AP.
              if (!IsEmpty(export.SpDocKey.KeyCase))
              {
                if (ReadLegalActionIncomeSourceIncomeSource2())
                {
                  export.SpDocKey.KeyIncomeSource =
                    entities.IncomeSource.Identifier;
                  local.BatchTimestampWorkArea.IefTimestamp =
                    entities.IncomeSource.Identifier;
                  UseLeCabConvertTimestamp();
                  local.FieldValue.Value =
                    local.BatchTimestampWorkArea.TextTimestamp;
                }
              }
              else if (ReadLegalActionIncomeSourceIncomeSource3())
              {
                export.SpDocKey.KeyIncomeSource =
                  entities.IncomeSource.Identifier;
                local.BatchTimestampWorkArea.IefTimestamp =
                  entities.IncomeSource.Identifier;
                UseLeCabConvertTimestamp();
                local.FieldValue.Value =
                  local.BatchTimestampWorkArea.TextTimestamp;
              }

              break;
            case "** FV":
              // mjr
              // --------------------------------------------------------------------------------
              // 04/21/2008
              // WR 302941	Added field for Family Violence for Multiple APs
              // ---------------------------------------------------------------------------------------------
              switch(TrimEnd(local.CurrentCaseRole.Type1))
              {
                case "AP":
                  local.FieldValue.Value =
                    local.CsePerson.FamilyViolenceIndicator ?? "";

                  if (!IsEmpty(local.CsePerson.FamilyViolenceIndicator))
                  {
                    local.ApFv.FamilyViolenceIndicator =
                      local.CsePerson.FamilyViolenceIndicator ?? "";
                  }
                  else
                  {
                    local.ApFv.FamilyViolenceIndicator = "";
                  }

                  break;
                case "CH":
                  local.FieldValue.Value = "USING FV";

                  break;
                default:
                  break;
              }

              break;
            case "** LOPS":
              local.FieldValue.Value = "USING LOPS";

              break;
            case "** LROL":
              local.FieldValue.Value = "USING LROL";

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "ATTORNEY":
          if (local.PersonPrivateAttorney.Identifier <= 0)
          {
            // mjr
            // ----------------------------------------------------
            // 05/28/1999
            // An attorney may be associated with a particular court case
            // -----------------------------------------------------------------
            if (export.SpDocKey.KeyLegalAction > 0)
            {
              if (ReadLegalAction())
              {
                local.LegalAction.CourtCaseNumber =
                  entities.LegalAction.CourtCaseNumber;
              }
              else
              {
                goto Test2;
              }

              if (!ReadTribunal())
              {
                goto Test2;
              }

              if (ReadFips())
              {
                MoveFips(entities.Fips, local.Fips);
              }
            }

Test2:

            if (Equal(local.CurrentCaseRole.Type1, "CH") && !
              Lt(local.CurrentNumberField.Name, "0") && !
              Lt("9", local.CurrentNumberField.Name))
            {
              for(local.Ch.Index = 0; local.Ch.Index < local.Ch.Count; ++
                local.Ch.Index)
              {
                if (!local.Ch.CheckSize())
                {
                  break;
                }

                if (ReadPersonPrivateAttorney2())
                {
                  local.CsePersonsWorkSet.Number =
                    local.Ch.Item.GlocalChCsePersonsWorkSet.Number;
                  local.PersonPrivateAttorney.Assign(
                    entities.PersonPrivateAttorney);

                  break;
                }
              }

              local.Ch.CheckIndex();
            }
            else
            {
              // 03/24/2006      M J Quinn       PR266315        SR5T is the 
              // GTSC screen.
              if (Equal(import.NextTranInfo.LastTran, "SR5T"))
              {
                if (ReadPersonPrivateAttorney1())
                {
                  local.PersonPrivateAttorney.Assign(
                    entities.PersonPrivateAttorney);
                }
              }
              else if (ReadPersonPrivateAttorney3())
              {
                local.PersonPrivateAttorney.Assign(
                  entities.PersonPrivateAttorney);
              }
            }
          }

          if (local.PersonPrivateAttorney.Identifier <= 0)
          {
            local.PreviousField.Name = local.CurrentCaseRole.Type1 + "ATZ";

            continue;
          }

          switch(TrimEnd(local.CurrentField.Name))
          {
            case "*ATADD1":
              local.ProcessGroup.Flag = "A";

              if (ReadPrivateAttorneyAddress())
              {
                if (Equal(entities.PrivateAttorneyAddress.Country, "US") || IsEmpty
                  (entities.PrivateAttorneyAddress.Country))
                {
                  local.SpPrintWorkSet.LocationType = "D";
                }
                else
                {
                  local.SpPrintWorkSet.LocationType = "F";
                }

                local.SpPrintWorkSet.Street1 =
                  entities.PrivateAttorneyAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.PrivateAttorneyAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.City =
                  entities.PrivateAttorneyAddress.City ?? Spaces(15);
                local.SpPrintWorkSet.State =
                  entities.PrivateAttorneyAddress.State ?? Spaces(2);
                local.SpPrintWorkSet.ZipCode =
                  entities.PrivateAttorneyAddress.ZipCode5 ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.PrivateAttorneyAddress.ZipCode4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  entities.PrivateAttorneyAddress.Zip3 ?? Spaces(3);
                local.SpPrintWorkSet.Country =
                  entities.PrivateAttorneyAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.PrivateAttorneyAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  entities.PrivateAttorneyAddress.Province ?? Spaces(5);
                UseSpDocFormatAddress();
              }

              break;
            case "*ATFIRMNM":
              local.FieldValue.Value = local.PersonPrivateAttorney.FirmName ?? ""
                ;

              break;
            case "*ATNM":
              local.SpPrintWorkSet.FirstName =
                local.PersonPrivateAttorney.FirstName ?? Spaces(12);
              local.SpPrintWorkSet.MidInitial =
                local.PersonPrivateAttorney.MiddleInitial ?? Spaces(1);
              local.SpPrintWorkSet.LastName =
                local.PersonPrivateAttorney.LastName ?? Spaces(17);
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "EMPLOYER":
          switch(TrimEnd(local.CurrentCaseRole.Type1))
          {
            case "AP":
              if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
              {
                // -- 09/16/2015 GVandy CQ22212 Employer fields must be 
                // retrieved for e-IWOs.
                if (Equal(import.Document.Name, "IWO") || Equal
                  (import.Document.Name, "IWOMODO") || Equal
                  (import.Document.Name, "ORDIWO2") || Equal
                  (import.Document.Name, "ORDIWO2A") || Equal
                  (import.Document.Name, "IWOTERM") || Equal
                  (import.Document.Name, "ORDIWOPT") || Equal
                  (import.Document.Name, "ORDIWOLS"))
                {
                  break;
                }

                goto Test5;
              }

              break;
            case "AR":
              if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
              {
                goto Test5;
              }

              break;
            case "CH":
              if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
              {
                goto Test5;
              }

              if (!IsEmpty(local.DetermineChFv.Name))
              {
                local.Ch.Index = 0;

                for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                  local.Ch.Index)
                {
                  if (!local.Ch.CheckSize())
                  {
                    break;
                  }

                  if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                    FamilyViolenceIndicator))
                  {
                    local.ChFv.FamilyViolenceIndicator =
                      local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator ?? "";
                    local.FieldValue.Value =
                      local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator ?? "";
                    UseSpCabCreateUpdateFieldValue3();

                    if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                    {
                      ExitState = "ACO_NN0000_ALL_OK";
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      export.ErrorDocumentField.ScreenPrompt = "Creation Error";
                      export.ErrorFieldValue.Value = "Field:  " + local
                        .Temp.Name;

                      return;
                    }

                    local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
                    ++import.ExpImpRowLockFieldValue.Count;

                    break;
                  }
                }

                local.Ch.CheckIndex();
                local.DetermineChFv.Name = "";
              }

              break;
            case "PR":
              if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
              {
                goto Test5;
              }

              break;
            default:
              break;
          }

          // mjr
          // --------------------------------------------
          // 09/18/2001
          // New income source requested
          // ---------------------------------------------------------
          if (Equal(local.IncomeSource.Identifier,
            local.NullDateWorkArea.Timestamp))
          {
            local.Employer.Identifier = 0;

            // mjr
            // --------------------------------------------
            // 09/18/2001
            // Find income source based on input, if available
            // ---------------------------------------------------------
            if (Lt(local.NullDateWorkArea.Timestamp,
              import.SpDocKey.KeyIncomeSource))
            {
              if (ReadIncomeSource5())
              {
                local.IncomeSource.Assign(entities.IncomeSource);

                goto Test3;
              }
              else
              {
                local.PreviousField.Name = local.CurrentCaseRole.Type1 + "EMZ";

                continue;
              }
            }

            // mjr
            // --------------------------------------------
            // 09/18/2001
            // Find latest income source for that person
            // ---------------------------------------------------------
            if (ReadIncomeSource3())
            {
              local.IncomeSource.Assign(entities.IncomeSource);

              goto Test3;
            }

            // mjr
            // --------------------------------------------
            // 09/18/2001
            // Income Source not found using any sources:  skip all fields
            // ---------------------------------------------------------
            if (Equal(local.CurrentField.Name, "**EMAD1") || Equal
              (local.CurrentField.Name, "**EMPNM"))
            {
              local.PreviousField.Name =
                TrimEnd(local.CurrentCaseRole.Type1) + NumberToString
                (local.CurrentNumberCommon.Count, 15, 1) + "EMPNZ";
            }
            else
            {
              local.PreviousField.Name = local.CurrentCaseRole.Type1 + "EMZ";
            }

            continue;
          }

Test3:

          // mjr
          // --------------------------------------------
          // 09/18/2001
          // If no income source was passed in and none other has been
          // used yet, use this one as the one that will be recorded
          // into infrastructure
          // ---------------------------------------------------------
          if (Equal(export.SpDocKey.KeyIncomeSource,
            local.NullDateWorkArea.Timestamp))
          {
            export.SpDocKey.KeyIncomeSource = local.IncomeSource.Identifier;
          }

          switch(TrimEnd(local.CurrentField.Name))
          {
            case "*EMPAD1":
              local.ProcessGroup.Flag = "A";

              if (ReadEmployerAddress2())
              {
                local.SpPrintWorkSet.LocationType =
                  entities.EmployerAddress.LocationType;
                local.SpPrintWorkSet.Street1 =
                  entities.EmployerAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.EmployerAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 =
                  entities.EmployerAddress.Street3 ?? Spaces(25);
                local.SpPrintWorkSet.Street4 =
                  entities.EmployerAddress.Street4 ?? Spaces(25);
                local.SpPrintWorkSet.City = entities.EmployerAddress.City ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = entities.EmployerAddress.State ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode =
                  entities.EmployerAddress.ZipCode ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 = entities.EmployerAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = entities.EmployerAddress.Zip3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Country =
                  entities.EmployerAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.EmployerAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  entities.EmployerAddress.Province ?? Spaces(5);
                UseSpDocFormatAddress();
              }

              break;
            case "*EMPEIN":
              if (ReadEmployer2())
              {
                local.Employer.Assign(entities.Employer);
              }
              else
              {
                break;
              }

              local.FieldValue.Value = entities.Employer.Ein;

              break;
            case "*EMPHQAD1":
              local.ProcessGroup.Flag = "A";

              if (local.Employer.Identifier == 0)
              {
                if (ReadEmployer2())
                {
                  local.Employer.Assign(entities.Employer);
                }
                else
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + "EMZ"
                    ;

                  continue;
                }
              }

              // *** November 29, 1999  David Lowry.
              // PR79759, use the HQ address if specified.
              if (Equal(entities.IncomeSource.SendTo, "HQ"))
              {
                if (ReadEmployerEmployerRelation())
                {
                  if (ReadEmployerAddress4())
                  {
                    local.SpPrintWorkSet.LocationType =
                      entities.EmployerAddress.LocationType;
                    local.SpPrintWorkSet.Street1 =
                      entities.EmployerAddress.Street1 ?? Spaces(25);
                    local.SpPrintWorkSet.Street2 =
                      entities.EmployerAddress.Street2 ?? Spaces(25);
                    local.SpPrintWorkSet.Street3 =
                      entities.EmployerAddress.Street3 ?? Spaces(25);
                    local.SpPrintWorkSet.Street4 =
                      entities.EmployerAddress.Street4 ?? Spaces(25);
                    local.SpPrintWorkSet.City =
                      entities.EmployerAddress.City ?? Spaces(15);
                    local.SpPrintWorkSet.State =
                      entities.EmployerAddress.State ?? Spaces(2);
                    local.SpPrintWorkSet.ZipCode =
                      entities.EmployerAddress.ZipCode ?? Spaces(5);
                    local.SpPrintWorkSet.Zip4 =
                      entities.EmployerAddress.Zip4 ?? Spaces(4);
                    local.SpPrintWorkSet.Zip3 =
                      entities.EmployerAddress.Zip3 ?? Spaces(3);
                    local.SpPrintWorkSet.Country =
                      entities.EmployerAddress.Country ?? Spaces(2);
                    local.SpPrintWorkSet.PostalCode =
                      entities.EmployerAddress.PostalCode ?? Spaces(10);
                    local.SpPrintWorkSet.Province =
                      entities.EmployerAddress.Province ?? Spaces(5);
                    UseSpDocFormatAddress();
                  }

                  break;
                }
                else
                {
                  // mjr
                  // ---------------------------------------------------
                  // Not an error - no verified HQ exists for this employer
                  // ------------------------------------------------------
                }
              }

              if (ReadEmployerAddress3())
              {
                local.SpPrintWorkSet.LocationType =
                  entities.EmployerAddress.LocationType;
                local.SpPrintWorkSet.Street1 =
                  entities.EmployerAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.EmployerAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 =
                  entities.EmployerAddress.Street3 ?? Spaces(25);
                local.SpPrintWorkSet.Street4 =
                  entities.EmployerAddress.Street4 ?? Spaces(25);
                local.SpPrintWorkSet.City = entities.EmployerAddress.City ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = entities.EmployerAddress.State ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode =
                  entities.EmployerAddress.ZipCode ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 = entities.EmployerAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = entities.EmployerAddress.Zip3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Country =
                  entities.EmployerAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.EmployerAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  entities.EmployerAddress.Province ?? Spaces(5);
                UseSpDocFormatAddress();
              }

              break;
            case "*EMPHQNM":
              if (local.Employer.Identifier == 0)
              {
                if (ReadEmployer2())
                {
                  local.Employer.Assign(entities.Employer);
                }
                else
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + "EMZ"
                    ;

                  continue;
                }
              }

              if (Equal(entities.IncomeSource.SendTo, "HQ"))
              {
                if (ReadEmployerEmployerRelation())
                {
                  local.FieldValue.Value = entities.Hq.Name;

                  break;
                }
                else
                {
                  // mjr
                  // ---------------------------------------------------
                  // Not an error - no verified HQ exists for this employer
                  // ------------------------------------------------------
                }
              }

              local.FieldValue.Value = local.Employer.Name ?? "";

              break;
            case "*EMPNM":
              if (local.Employer.Identifier == 0)
              {
                if (ReadEmployer2())
                {
                  local.Employer.Assign(entities.Employer);
                }
                else
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + "EMZ"
                    ;

                  continue;
                }
              }

              local.FieldValue.Value = local.Employer.Name ?? "";

              break;
            case "*EMPVRDT":
              if (Lt(local.NullDateWorkArea.Date, local.IncomeSource.ReturnDt) &&
                Lt(import.Current.Date, local.IncomeSource.EndDt))
              {
                local.DateWorkArea.Date = local.IncomeSource.ReturnDt;
                local.FieldValue.Value = UseSpDocFormatDate2();
              }

              break;
            case "*EMPPHONE":
              if (local.Employer.Identifier == 0)
              {
                if (ReadEmployer2())
                {
                  local.Employer.Assign(entities.Employer);
                }
                else
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + "EMZ"
                    ;

                  continue;
                }
              }

              local.SpPrintWorkSet.PhoneAreaCode =
                local.Employer.AreaCode.GetValueOrDefault();
              local.BatchConvertNumToText.Number15 =
                StringToNumber(local.Employer.PhoneNo);
              local.SpPrintWorkSet.Phone7Digit =
                (int)local.BatchConvertNumToText.Number15;
              local.SpPrintWorkSet.PhoneExt = "";
              local.FieldValue.Value = UseSpDocFormatPhoneNumber();

              break;
            case "*RSDNTAD1":
              // 03/07/2002 G Vandy  PR123074  Process resident agent address as
              // an address group
              local.ProcessGroup.Flag = "A";

              if (local.Employer.Identifier == 0)
              {
                if (ReadEmployer2())
                {
                  local.Employer.Assign(entities.Employer);
                }
                else
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + "RSDNTZ"
                    ;

                  continue;
                }
              }

              if (!ReadEmployerRegisteredAgent())
              {
                local.PreviousField.Name = local.CurrentCaseRole.Type1 + "RSDNTZ"
                  ;

                continue;
              }

              if (ReadRegisteredAgentAddress())
              {
                local.SpPrintWorkSet.LocationType = "D";
                local.SpPrintWorkSet.Street1 =
                  entities.RegisteredAgentAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.RegisteredAgentAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.City =
                  entities.RegisteredAgentAddress.City ?? Spaces(15);
                local.SpPrintWorkSet.State =
                  entities.RegisteredAgentAddress.State ?? Spaces(2);
                local.SpPrintWorkSet.ZipCode =
                  entities.RegisteredAgentAddress.ZipCode5 ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.RegisteredAgentAddress.ZipCode4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  entities.RegisteredAgentAddress.Zip3 ?? Spaces(3);
                UseSpDocFormatAddress();
              }

              if (IsEmpty(entities.RegisteredAgentAddress.Street1))
              {
                local.PreviousField.Name = local.CurrentCaseRole.Type1 + "RSDNTAD5"
                  ;

                continue;
              }

              break;
            case "*RSDNTNM":
              if (local.Employer.Identifier == 0)
              {
                if (ReadEmployer2())
                {
                  local.Employer.Assign(entities.Employer);
                }
                else
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + "RSDNTZ"
                    ;

                  continue;
                }
              }

              if (IsEmpty(entities.EmployerRegisteredAgent.Identifier))
              {
                if (!ReadEmployerRegisteredAgent())
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + "RSDNTZ"
                    ;

                  continue;
                }
              }

              if (ReadRegisteredAgent())
              {
                local.FieldValue.Value = entities.RegisteredAgent.Name;
              }

              break;
            case "**EMAD1":
              local.ProcessGroup.Flag = "A";

              if (ReadEmployerAddress2())
              {
                local.SpPrintWorkSet.LocationType =
                  entities.EmployerAddress.LocationType;
                local.SpPrintWorkSet.Street1 =
                  entities.EmployerAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.EmployerAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 =
                  entities.EmployerAddress.Street3 ?? Spaces(25);
                local.SpPrintWorkSet.Street4 =
                  entities.EmployerAddress.Street4 ?? Spaces(25);
                local.SpPrintWorkSet.City = entities.EmployerAddress.City ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = entities.EmployerAddress.State ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode =
                  entities.EmployerAddress.ZipCode ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 = entities.EmployerAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = entities.EmployerAddress.Zip3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Country =
                  entities.EmployerAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.EmployerAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  entities.EmployerAddress.Province ?? Spaces(5);
                UseSpDocFormatAddress();
              }

              break;
            case "**EMPNM":
              if (ReadEmployer2())
              {
                local.FieldValue.Value = entities.Employer.Name;
              }

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "INCMSRCE":
          // *********************************************************************************
          // 08/10/07  G. Pan   PR310519
          //                    Commented out following codes for case 'AP', '
          // AR' or 'CH'.
          // *********************************************************************************
          switch(TrimEnd(local.CurrentCaseRole.Type1))
          {
            case "AP":
              break;
            case "AR":
              break;
            case "CH":
              if (!IsEmpty(local.DetermineChFv.Name))
              {
                local.Ch.Index = 0;

                for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                  local.Ch.Index)
                {
                  if (!local.Ch.CheckSize())
                  {
                    break;
                  }

                  if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                    FamilyViolenceIndicator))
                  {
                    local.ChFv.FamilyViolenceIndicator =
                      local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator ?? "";
                    local.FieldValue.Value =
                      local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator ?? "";
                    UseSpCabCreateUpdateFieldValue3();

                    if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                    {
                      ExitState = "ACO_NN0000_ALL_OK";
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      export.ErrorDocumentField.ScreenPrompt = "Creation Error";
                      export.ErrorFieldValue.Value = "Field:  " + local
                        .Temp.Name;

                      return;
                    }

                    local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
                    ++import.ExpImpRowLockFieldValue.Count;

                    break;
                  }
                }

                local.Ch.CheckIndex();
                local.DetermineChFv.Name = "";
              }

              break;
            case "PR":
              if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
              {
                goto Test5;
              }

              break;
            default:
              break;
          }

          if (!Lt(local.NullDateWorkArea.Timestamp,
            export.SpDocKey.KeyIncomeSource))
          {
            // 06/30/2003  GVandy  PR176328  If income source id is not 
            // populated default to most recent non end dated income source.
            if (ReadIncomeSource1())
            {
              export.SpDocKey.KeyIncomeSource =
                entities.IncomeSource.Identifier;
            }
          }
          else if (ReadIncomeSource4())
          {
            export.SpDocKey.KeyIncomeSource = entities.IncomeSource.Identifier;
          }

          if (!Lt(local.NullDateWorkArea.Timestamp,
            export.SpDocKey.KeyIncomeSource))
          {
            local.PreviousField.Name = local.CurrentCaseRole.Type1 + "INCZ";

            continue;
          }

          switch(TrimEnd(local.CurrentField.Name))
          {
            case "*INCSADD1":
              local.ProcessGroup.Flag = "A";

              // --11-21-2018  GVandy  CQ61457  SSA changes.
              if (AsChar(entities.IncomeSource.Type1) == 'O' && !
                Equal(entities.IncomeSource.Code, "SA") || AsChar
                (entities.IncomeSource.Type1) == 'R')
              {
                if (ReadNonEmployIncomeSourceAddress2())
                {
                  local.SpPrintWorkSet.LocationType =
                    entities.NonEmployIncomeSourceAddress.LocationType;
                  local.SpPrintWorkSet.Street1 =
                    entities.NonEmployIncomeSourceAddress.Street1 ?? Spaces
                    (25);
                  local.SpPrintWorkSet.Street2 =
                    entities.NonEmployIncomeSourceAddress.Street2 ?? Spaces
                    (25);
                  local.SpPrintWorkSet.Street3 =
                    entities.NonEmployIncomeSourceAddress.Street3 ?? Spaces
                    (25);
                  local.SpPrintWorkSet.Street4 =
                    entities.NonEmployIncomeSourceAddress.Street4 ?? Spaces
                    (25);
                  local.SpPrintWorkSet.City =
                    entities.NonEmployIncomeSourceAddress.City ?? Spaces(15);
                  local.SpPrintWorkSet.State =
                    entities.NonEmployIncomeSourceAddress.State ?? Spaces(2);
                  local.SpPrintWorkSet.ZipCode =
                    entities.NonEmployIncomeSourceAddress.ZipCode ?? Spaces(5);
                  local.SpPrintWorkSet.Zip4 =
                    entities.NonEmployIncomeSourceAddress.Zip4 ?? Spaces(4);
                  local.SpPrintWorkSet.Zip3 =
                    entities.NonEmployIncomeSourceAddress.Zip3 ?? Spaces(3);
                  local.SpPrintWorkSet.Country =
                    entities.NonEmployIncomeSourceAddress.Country ?? Spaces(2);
                  local.SpPrintWorkSet.PostalCode =
                    entities.NonEmployIncomeSourceAddress.PostalCode ?? Spaces
                    (10);
                  local.SpPrintWorkSet.Province =
                    entities.NonEmployIncomeSourceAddress.Province ?? Spaces
                    (5);
                }
              }
              else if (ReadEmployerAddress1())
              {
                local.SpPrintWorkSet.LocationType =
                  entities.EmployerAddress.LocationType;
                local.SpPrintWorkSet.Street1 =
                  entities.EmployerAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.EmployerAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 =
                  entities.EmployerAddress.Street3 ?? Spaces(25);
                local.SpPrintWorkSet.Street4 =
                  entities.EmployerAddress.Street4 ?? Spaces(25);
                local.SpPrintWorkSet.City = entities.EmployerAddress.City ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = entities.EmployerAddress.State ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode =
                  entities.EmployerAddress.ZipCode ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 = entities.EmployerAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = entities.EmployerAddress.Zip3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Country =
                  entities.EmployerAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.EmployerAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  entities.EmployerAddress.Province ?? Spaces(5);
              }

              if (!IsEmpty(local.SpPrintWorkSet.Street1))
              {
                UseSpDocFormatAddress();
              }

              break;
            case "*INCSNM":
              // --11-21-2018  GVandy  CQ61457  SSA changes.
              if (AsChar(entities.IncomeSource.Type1) == 'O' && !
                Equal(entities.IncomeSource.Code, "SA") || AsChar
                (entities.IncomeSource.Type1) == 'R')
              {
                local.FieldValue.Value = entities.IncomeSource.Name;
              }
              else if (ReadEmployer1())
              {
                local.FieldValue.Value = entities.Employer.Name;
              }

              break;
            default:
              break;
          }

          break;
        case "PERSON":
          // mjr
          // ----------------------------------------------------
          // Determine what data is needed for the field
          // -------------------------------------------------------
          if (Equal(local.CurrentField.Name, "*ADDRID") || Equal
            (local.CurrentField.Name, "*ADDRVRDT") || Equal
            (local.CurrentField.Name, "*ADDR1") || Equal
            (local.CurrentField.Name, "*CITY") || Equal
            (local.CurrentField.Name, "*STATE") || Equal
            (local.CurrentField.Name, "*STRT1") || Equal
            (local.CurrentField.Name, "*STRT2") || Equal
            (local.CurrentField.Name, "*ZIP") || Equal
            (local.CurrentField.Name, "**ADDR1"))
          {
            local.ProcessGroup.Flag = "A";

            switch(TrimEnd(local.CurrentCaseRole.Type1))
            {
              case "AP":
                if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
                {
                  goto Test5;
                }

                break;
              case "AR":
                if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
                {
                  goto Test5;
                }

                break;
              case "CH":
                if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
                {
                  goto Test5;
                }

                if (!IsEmpty(local.DetermineChFv.Name))
                {
                  local.Ch.Index = 0;

                  for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                    local.Ch.Index)
                  {
                    if (!local.Ch.CheckSize())
                    {
                      break;
                    }

                    if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                      FamilyViolenceIndicator))
                    {
                      local.ChFv.FamilyViolenceIndicator =
                        local.Ch.Item.GlocalChCsePerson.
                          FamilyViolenceIndicator ?? "";
                      local.FieldValue.Value =
                        local.Ch.Item.GlocalChCsePerson.
                          FamilyViolenceIndicator ?? "";
                      UseSpCabCreateUpdateFieldValue3();

                      if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                      {
                        ExitState = "ACO_NN0000_ALL_OK";
                      }

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        export.ErrorDocumentField.ScreenPrompt =
                          "Creation Error";
                        export.ErrorFieldValue.Value = "Field:  " + local
                          .Temp.Name;

                        return;
                      }

                      local.FieldValue.Value =
                        Spaces(FieldValue.Value_MaxLength);
                      ++import.ExpImpRowLockFieldValue.Count;

                      break;
                    }
                  }

                  local.Ch.CheckIndex();
                  local.DetermineChFv.Name = "";
                }

                break;
              case "PR":
                if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
                {
                  goto Test5;
                }

                break;
              default:
                break;
            }

            if (Equal(local.CsePersonsWorkSet.Number, "AAPPEAL"))
            {
              // mjr
              // ----------------------------------------------------
              // Use Appellant Name and Appellant Address
              // -------------------------------------------------------
              if (ReadAdminAppealAppellantAddress())
              {
                if (IsEmpty(entities.AdminAppealAppellantAddress.Country) || Equal
                  (entities.AdminAppealAppellantAddress.Country, "US"))
                {
                  local.SpPrintWorkSet.LocationType = "D";
                }
                else
                {
                  local.SpPrintWorkSet.LocationType = "F";
                }

                local.SpPrintWorkSet.Street1 =
                  entities.AdminAppealAppellantAddress.Street1;
                local.SpPrintWorkSet.Street2 =
                  entities.AdminAppealAppellantAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 = "";
                local.SpPrintWorkSet.Street4 = "";
                local.SpPrintWorkSet.City =
                  entities.AdminAppealAppellantAddress.City;
                local.SpPrintWorkSet.State =
                  entities.AdminAppealAppellantAddress.StateProvince;
                local.SpPrintWorkSet.ZipCode =
                  entities.AdminAppealAppellantAddress.ZipCode;
                local.SpPrintWorkSet.Zip4 =
                  entities.AdminAppealAppellantAddress.Zip4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  entities.AdminAppealAppellantAddress.Zip3 ?? Spaces(3);
                local.SpPrintWorkSet.Country =
                  entities.AdminAppealAppellantAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.AdminAppealAppellantAddress.PostalCode ?? Spaces
                  (10);
                local.SpPrintWorkSet.Province = "";
              }

              if (IsEmpty(entities.AdminAppealAppellantAddress.Street1))
              {
              }
            }
            else if (Equal(local.CsePersonsWorkSet.Number, "INFO REQ"))
            {
              // mjr
              // ----------------------------------------------------
              // Use Information Request Applicant Name and Applicant Address
              // -------------------------------------------------------
              if (!ReadInformationRequest())
              {
                goto Test4;
              }

              local.SpPrintWorkSet.LocationType = "D";
              local.SpPrintWorkSet.Street1 =
                entities.InformationRequest.ApplicantStreet1 ?? Spaces(25);
              local.SpPrintWorkSet.Street2 =
                entities.InformationRequest.ApplicantStreet2 ?? Spaces(25);
              local.SpPrintWorkSet.Street3 = "";
              local.SpPrintWorkSet.Street4 = "";
              local.SpPrintWorkSet.City =
                entities.InformationRequest.ApplicantCity ?? Spaces(15);
              local.SpPrintWorkSet.State =
                entities.InformationRequest.ApplicantState ?? Spaces(2);
              local.SpPrintWorkSet.ZipCode =
                entities.InformationRequest.ApplicantZip5 ?? Spaces(5);
              local.SpPrintWorkSet.Zip4 =
                entities.InformationRequest.ApplicantZip4 ?? Spaces(4);
              local.SpPrintWorkSet.Zip3 =
                entities.InformationRequest.ApplicantZip3 ?? Spaces(3);
              local.SpPrintWorkSet.Country = "";
              local.SpPrintWorkSet.PostalCode = "";
              local.SpPrintWorkSet.Province = "";
            }
            else if (Equal(local.CsePersonsWorkSet.Number, "HEALTHINS"))
            {
              // mjr
              // ----------------------------------------------------
              // Use Contact Name and Contact Address
              // -------------------------------------------------------
              if (ReadContactAddress())
              {
                if (IsEmpty(entities.ContactAddress.Country) || Equal
                  (entities.ContactAddress.Country, "US"))
                {
                  local.SpPrintWorkSet.LocationType = "D";
                }
                else
                {
                  local.SpPrintWorkSet.LocationType = "F";
                }

                local.SpPrintWorkSet.Street1 =
                  entities.ContactAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.ContactAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 = "";
                local.SpPrintWorkSet.Street4 = "";
                local.SpPrintWorkSet.City = entities.ContactAddress.City ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = entities.ContactAddress.State ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode =
                  entities.ContactAddress.ZipCode5 ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.ContactAddress.ZipCode4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 = entities.ContactAddress.Zip3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Country =
                  entities.ContactAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.ContactAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  entities.ContactAddress.Province ?? Spaces(5);
              }

              if (IsEmpty(entities.ContactAddress.Street1))
              {
              }
            }
            else
            {
              if (Lt(local.NullDateWorkArea.Timestamp,
                export.SpDocKey.KeyPersonAddress))
              {
                if (ReadCsePersonAddress())
                {
                  MoveCsePersonAddress(entities.CsePersonAddress,
                    local.CsePersonAddress);
                }
                else
                {
                  goto Test4;
                }
              }
              else
              {
                UseSiGetCsePersonMailingAddr2();

                if (IsEmpty(local.CsePersonAddress.LocationType))
                {
                  goto Test4;
                }
              }

              local.SpPrintWorkSet.LocationType =
                local.CsePersonAddress.LocationType;
              local.SpPrintWorkSet.Street1 = local.CsePersonAddress.Street1 ?? Spaces
                (25);
              local.SpPrintWorkSet.Street2 = local.CsePersonAddress.Street2 ?? Spaces
                (25);
              local.SpPrintWorkSet.Street3 = local.CsePersonAddress.Street3 ?? Spaces
                (25);
              local.SpPrintWorkSet.Street4 = local.CsePersonAddress.Street4 ?? Spaces
                (25);
              local.SpPrintWorkSet.City = local.CsePersonAddress.City ?? Spaces
                (15);
              local.SpPrintWorkSet.State = local.CsePersonAddress.State ?? Spaces
                (2);
              local.SpPrintWorkSet.ZipCode = local.CsePersonAddress.ZipCode ?? Spaces
                (5);
              local.SpPrintWorkSet.Zip4 = local.CsePersonAddress.Zip4 ?? Spaces
                (4);
              local.SpPrintWorkSet.Zip3 = local.CsePersonAddress.Zip3 ?? Spaces
                (3);
              local.SpPrintWorkSet.Country = local.CsePersonAddress.Country ?? Spaces
                (2);
              local.SpPrintWorkSet.PostalCode =
                local.CsePersonAddress.PostalCode ?? Spaces(10);
              local.SpPrintWorkSet.Province =
                local.CsePersonAddress.Province ?? Spaces(5);
            }
          }
          else if (Equal(local.CurrentField.Name, "*DEATHDT") || Equal
            (local.CurrentField.Name, "*DOB") || Equal
            (local.CurrentField.Name, "*EYE") || Equal
            (local.CurrentField.Name, "*FIRSTNM") || Equal
            (local.CurrentField.Name, "*HAIR") || Equal
            (local.CurrentField.Name, "*HEIGHT") || Equal
            (local.CurrentField.Name, "*HOMEPH") || Equal
            (local.CurrentField.Name, "*LASTNM") || Equal
            (local.CurrentField.Name, "*MIDINIT") || Equal
            (local.CurrentField.Name, "*NM") || Equal
            (local.CurrentField.Name, "*NMORGZ") || Equal
            (local.CurrentField.Name, "*NUMBER") || Equal
            (local.CurrentField.Name, "*POB") || Equal
            (local.CurrentField.Name, "*RACE") || Equal
            (local.CurrentField.Name, "*SEX") || Equal
            (local.CurrentField.Name, "*SSN") || Equal
            (local.CurrentField.Name, "*SSNXX") || Equal
            (local.CurrentField.Name, "*WEIGHT") || Equal
            (local.CurrentField.Name, "**AGE") || Equal
            (local.CurrentField.Name, "**DOB") || Equal
            (local.CurrentField.Name, "**HOMEPH") || Equal
            (local.CurrentField.Name, "**NM") || Equal
            (local.CurrentField.Name, "**POB") || Equal
            (local.CurrentField.Name, "**SEX") || Equal
            (local.CurrentField.Name, "**SSN") || Equal
            (local.CurrentField.Name, "**SSNXX"))
          {
            // mjr
            // --------------------------------------------------------------------------------
            // 03/04/2008
            // WR 276, 277	Added fields for masked SSNs  (*SSNXX, **SSNXX, *
            // MOSSNXX, **MOSSNXX)
            // Added appropriate conditions to ELSE IF above
            // ---------------------------------------------------------------------------------------------
            // mjr
            // --------------------------------------------------------------------------------
            // 04/11/2008
            // WR 302941	Added fields for DET2PATP (Multiple APs Home Phone, 
            // Sex, SSN, DOB)
            // Added appropriate conditions to ELSE IF above (all already 
            // existed except **HOMEPH)
            // ---------------------------------------------------------------------------------------------
            // nop
          }
          else if (Equal(local.CurrentField.Name, "*MODOB") || Equal
            (local.CurrentField.Name, "*MOFRSTNM") || Equal
            (local.CurrentField.Name, "*MOLASTNM") || Equal
            (local.CurrentField.Name, "*MOMIDDLI") || Equal
            (local.CurrentField.Name, "*MOSSN") || Equal
            (local.CurrentField.Name, "*MOSSNXX") || Equal
            (local.CurrentField.Name, "*MTHRNM") || Equal
            (local.CurrentField.Name, "**MODOB") || Equal
            (local.CurrentField.Name, "**MOHOPHO") || Equal
            (local.CurrentField.Name, "**MOPOB") || Equal
            (local.CurrentField.Name, "**MOSSN") || Equal
            (local.CurrentField.Name, "**MOSSNXX") || Equal
            (local.CurrentField.Name, "**MTHRNM") || Equal
            (local.CurrentField.Name, "*FADOB") || Equal
            (local.CurrentField.Name, "*FAFRSTNM") || Equal
            (local.CurrentField.Name, "*FALASTNM") || Equal
            (local.CurrentField.Name, "*FAMIDDLI") || Equal
            (local.CurrentField.Name, "*FASSN") || Equal
            (local.CurrentField.Name, "*FASSNXX") || Equal
            (local.CurrentField.Name, "*FTHRNM") || Equal
            (local.CurrentField.Name, "**FADOB") || Equal
            (local.CurrentField.Name, "**FAHOPHO") || Equal
            (local.CurrentField.Name, "**FAPOB") || Equal
            (local.CurrentField.Name, "**FASSN") || Equal
            (local.CurrentField.Name, "**FASSNXX") || Equal
            (local.CurrentField.Name, "**FTHRNM"))
          {
            // mjr
            // --------------------------------------------------------------------------------
            // 03/04/2008
            // WR 276, 277	Added fields for masked SSNs  (*SSNXX, **SSNXX, *
            // MOSSNXX, **MOSSNXX)
            // Added appropriate conditions to ELSE IF above
            // ---------------------------------------------------------------------------------------------
            if (Equal(local.CurrentField.Name, 1, 2, "*M") || Equal
              (local.CurrentField.Name, 1, 3, "**M"))
            {
              local.NaturalParentCaseRole.Type1 = "MO";
            }
            else
            {
              local.NaturalParentCaseRole.Type1 = "FA";
            }

            if (IsEmpty(local.NaturalParentCsePersonsWorkSet.Number) || AsChar
              (local.GetPersonNameFlag.Flag) == 'Y')
            {
              if (IsEmpty(entities.NaturalParentCsePerson.Number) || AsChar
                (local.GetPersonNameFlag.Flag) == 'Y')
              {
                if (Equal(local.CurrentCaseRole.Type1, "CH") && !
                  Lt(local.CurrentNumberField.Name, "0") && !
                  Lt("9", local.CurrentNumberField.Name))
                {
                  for(local.Ch.Index = 0; local.Ch.Index < local.Ch.Count; ++
                    local.Ch.Index)
                  {
                    if (!local.Ch.CheckSize())
                    {
                      break;
                    }

                    if (ReadCsePersonCaseRole2())
                    {
                      local.GetPersonNameFlag.Flag = "";

                      break;
                    }
                  }

                  local.Ch.CheckIndex();
                }
                else if (ReadCsePersonCaseRole3())
                {
                  local.GetPersonNameFlag.Flag = "";
                }
              }

              if (IsEmpty(entities.NaturalParentCsePerson.Number))
              {
                if (Equal(local.CurrentCaseRole.Type1, "CH") && !
                  Lt(local.CurrentNumberField.Name, "0") && !
                  Lt("9", local.CurrentNumberField.Name))
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + "0"
                    + local.NaturalParentCaseRole.Type1 + "ZZ";
                }
                else
                {
                  local.PreviousField.Name = local.CurrentCaseRole.Type1 + local
                    .NaturalParentCaseRole.Type1 + "ZZ";
                }

                continue;
              }

              local.NaturalParentCsePersonsWorkSet.Number =
                entities.NaturalParentCsePerson.Number;
              export.SpDocKey.KeyPerson =
                local.NaturalParentCsePersonsWorkSet.Number;
              local.NaturalParentCaseRole.Type1 = "PR";
              UseSpDocGetPerson4();

              if (!IsEmpty(local.Read.Type1) || !
                IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ALL_OK";
              }
            }
          }
          else if (Equal(local.CurrentField.Name, "*MOCITY") || Equal
            (local.CurrentField.Name, "*MOSTATE") || Equal
            (local.CurrentField.Name, "*MOSTR1") || Equal
            (local.CurrentField.Name, "*MOSTR2") || Equal
            (local.CurrentField.Name, "*MOZIP") || Equal
            (local.CurrentField.Name, "**MOADDR1") || Equal
            (local.CurrentField.Name, "*FACITY") || Equal
            (local.CurrentField.Name, "*FASTATE") || Equal
            (local.CurrentField.Name, "*FASTR1") || Equal
            (local.CurrentField.Name, "*FASTR2") || Equal
            (local.CurrentField.Name, "*FAZIP") || Equal
            (local.CurrentField.Name, "**FAADDR1"))
          {
            if (Equal(local.CurrentField.Name, 1, 2, "*M") || Equal
              (local.CurrentField.Name, 1, 3, "**M"))
            {
              local.NaturalParentCaseRole.Type1 = "MO";
            }
            else
            {
              local.NaturalParentCaseRole.Type1 = "FA";
            }

            switch(TrimEnd(local.CurrentCaseRole.Type1))
            {
              case "AP":
                if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
                {
                  goto Test4;
                }

                break;
              case "AR":
                if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
                {
                  goto Test4;
                }

                break;
              case "CH":
                if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
                {
                  goto Test4;
                }

                if (!IsEmpty(local.DetermineChFv.Name))
                {
                  local.Ch.Index = 0;

                  for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                    local.Ch.Index)
                  {
                    if (!local.Ch.CheckSize())
                    {
                      break;
                    }

                    if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                      FamilyViolenceIndicator))
                    {
                      local.ChFv.FamilyViolenceIndicator =
                        local.Ch.Item.GlocalChCsePerson.
                          FamilyViolenceIndicator ?? "";
                      local.FieldValue.Value =
                        local.Ch.Item.GlocalChCsePerson.
                          FamilyViolenceIndicator ?? "";
                      UseSpCabCreateUpdateFieldValue3();

                      if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                      {
                        ExitState = "ACO_NN0000_ALL_OK";
                      }

                      if (!IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        export.ErrorDocumentField.ScreenPrompt =
                          "Creation Error";
                        export.ErrorFieldValue.Value = "Field:  " + local
                          .Temp.Name;

                        return;
                      }

                      local.FieldValue.Value =
                        Spaces(FieldValue.Value_MaxLength);
                      ++import.ExpImpRowLockFieldValue.Count;

                      break;
                    }
                  }

                  local.Ch.CheckIndex();
                  local.DetermineChFv.Name = "";
                }

                break;
              case "PR":
                if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
                {
                  goto Test4;
                }

                break;
              default:
                break;
            }

            if (IsEmpty(local.NaturalParentCsePersonAddress.Street1) || AsChar
              (local.GetPersonNameFlag.Flag) == 'Y')
            {
              if (IsEmpty(entities.NaturalParentCsePerson.Number) || AsChar
                (local.GetPersonNameFlag.Flag) == 'Y')
              {
                if (Equal(local.CurrentCaseRole.Type1, "CH") && !
                  Lt(local.CurrentNumberField.Name, "0") && !
                  Lt("9", local.CurrentNumberField.Name))
                {
                  for(local.Ch.Index = 0; local.Ch.Index < local.Ch.Count; ++
                    local.Ch.Index)
                  {
                    if (!local.Ch.CheckSize())
                    {
                      break;
                    }

                    if (ReadCsePersonCaseRole1())
                    {
                      local.GetPersonNameFlag.Flag = "";

                      break;
                    }
                  }

                  local.Ch.CheckIndex();
                }
                else if (ReadCsePersonCaseRole3())
                {
                  local.GetPersonNameFlag.Flag = "";
                }

                if (IsEmpty(entities.NaturalParentCsePerson.Number))
                {
                  if (Equal(local.CurrentCaseRole.Type1, "CH") && !
                    Lt(local.CurrentNumberField.Name, "0") && !
                    Lt("9", local.CurrentNumberField.Name))
                  {
                    local.PreviousField.Name = local.CurrentCaseRole.Type1 + "0"
                      + local.NaturalParentCaseRole.Type1 + "ZZ";
                  }
                  else
                  {
                    local.PreviousField.Name = local.CurrentCaseRole.Type1 + local
                      .CurrentField.Name + "ZZ";
                  }

                  continue;
                }
              }

              // 03/18/2010	J Huss	If the mother has a FVI set, do not retrieve 
              // the address.
              if (!IsEmpty(entities.NaturalParentCsePerson.
                FamilyViolenceIndicator))
              {
                goto Test4;
              }

              UseSiGetCsePersonMailingAddr1();
            }
          }
          else
          {
            // nop
          }

Test4:

          switch(TrimEnd(local.CurrentField.Name))
          {
            case "*ADDRID":
              // mjr---> Process group flag was already set to 'A'.  Reset,
              // 	since this is a single field.
              local.ProcessGroup.Flag = "N";
              local.BatchTimestampWorkArea.IefTimestamp =
                local.CsePersonAddress.Identifier;
              UseLeCabConvertTimestamp();
              local.FieldValue.Value =
                local.BatchTimestampWorkArea.TextTimestamp;

              break;
            case "*ADDR1":
              UseSpDocFormatAddress();

              break;
            case "*ADDRVRDT":
              local.ProcessGroup.Flag = "N";

              if (Lt(local.NullDateWorkArea.Date,
                local.CsePersonAddress.VerifiedDate) && Lt
                (import.Current.Date, local.CsePersonAddress.EndDate))
              {
                local.DateWorkArea.Date = local.CsePersonAddress.VerifiedDate;
                local.FieldValue.Value = UseSpDocFormatDate2();
              }

              break;
            case "*CITY":
              local.ProcessGroup.Flag = "";
              local.FieldValue.Value = local.CsePersonAddress.City ?? "";

              break;
            case "*COUNT":
              if (local.Ch.Count <= 0)
              {
                local.FieldValue.Value = "0";
              }
              else
              {
                local.FieldValue.Value =
                  NumberToString(local.Ch.Count,
                  Verify(NumberToString(local.Ch.Count, 15), "0"), 16 -
                  Verify(NumberToString(local.Ch.Count, 15), "0"));
              }

              break;
            case "*DEATHDT":
              if (Lt(local.NullDateWorkArea.Date, local.CsePerson.DateOfDeath))
              {
                local.DateWorkArea.Date = local.CsePerson.DateOfDeath;
                local.FieldValue.Value = UseSpDocFormatDate2();
              }

              break;
            case "*DOB":
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              if (AsChar(local.CsePerson.Type1) != 'O')
              {
                local.DateWorkArea.Date = local.CsePersonsWorkSet.Dob;
                UseSpDocFormatDate1();
              }

              break;
            case "*EYE":
              if (!IsEmpty(local.CsePerson.EyeColor))
              {
                local.Code.CodeName = "EYE COLOR";
                local.CodeValue.Cdvalue = local.CsePerson.EyeColor ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "*FACITY":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.City ?? "";

              break;
            case "*FADOB":
              if (AsChar(local.CsePerson.Type1) != 'O')
              {
                local.DateWorkArea.Date =
                  local.NaturalParentCsePersonsWorkSet.Dob;
                UseSpDocFormatDate1();
              }

              break;
            case "*FAFRSTNM":
              local.FieldValue.Value =
                local.NaturalParentCsePersonsWorkSet.FirstName;

              break;
            case "*FALASTNM":
              local.FieldValue.Value =
                local.NaturalParentCsePersonsWorkSet.LastName;

              break;
            case "*FAMIDDLI":
              local.FieldValue.Value =
                local.NaturalParentCsePersonsWorkSet.MiddleInitial;

              break;
            case "*FASSN":
              UseSpDocFormatSsn4();

              break;
            case "*FASSNXX":
              UseSpDocFormatSsn2();

              break;
            case "*FASTATE":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.State ?? "";

              break;
            case "*FASTR1":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.Street1 ?? "";

              break;
            case "*FASTR2":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.Street2 ?? "";

              break;
            case "*FAZIP":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.ZipCode ?? "";

              if (!IsEmpty(local.NaturalParentCsePersonAddress.Zip4))
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "-"
                  + (local.NaturalParentCsePersonAddress.Zip4 ?? "");

                if (!IsEmpty(local.NaturalParentCsePersonAddress.Zip3))
                {
                  local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "-"
                    + (local.NaturalParentCsePersonAddress.Zip3 ?? "");
                }
              }

              break;
            case "*FIRSTNM":
              local.FieldValue.Value = local.CsePersonsWorkSet.FirstName;

              break;
            case "*FTHRNM":
              local.SpPrintWorkSet.FirstName =
                local.NaturalParentCsePersonsWorkSet.FirstName;
              local.SpPrintWorkSet.MidInitial =
                local.NaturalParentCsePersonsWorkSet.MiddleInitial;
              local.SpPrintWorkSet.LastName =
                local.NaturalParentCsePersonsWorkSet.LastName;
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            case "*HAIR":
              if (!IsEmpty(local.CsePerson.HairColor))
              {
                local.Code.CodeName = "HAIR COLOR";
                local.CodeValue.Cdvalue = local.CsePerson.HairColor ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "*HEIGHT":
              if (local.CsePerson.HeightFt.GetValueOrDefault() > 0)
              {
                local.BatchConvertNumToText.TextNumber15 =
                  NumberToString(local.CsePerson.HeightFt.GetValueOrDefault(),
                  15);
                local.Position.Count =
                  Verify(local.BatchConvertNumToText.TextNumber15, "0");
                local.WorkArea.Text50 =
                  Substring(local.BatchConvertNumToText.TextNumber15,
                  local.Position.Count, 16 - local.Position.Count);
                local.FieldValue.Value = TrimEnd(local.WorkArea.Text50) + " ft.";
                  

                if (local.CsePerson.HeightIn.GetValueOrDefault() > 0)
                {
                  local.BatchConvertNumToText.TextNumber15 =
                    NumberToString(local.CsePerson.HeightIn.GetValueOrDefault(),
                    15);
                  local.Position.Count =
                    Verify(local.BatchConvertNumToText.TextNumber15, "0");
                  local.WorkArea.Text50 =
                    Substring(local.BatchConvertNumToText.TextNumber15,
                    local.Position.Count, 16 - local.Position.Count);
                  local.WorkArea.Text50 = TrimEnd(local.WorkArea.Text50) + " in.";
                    
                }
                else
                {
                  local.WorkArea.Text50 = "0 in.";
                }

                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + " " +
                  local.WorkArea.Text50;
              }

              break;
            case "*HOMEPH":
              switch(TrimEnd(local.CurrentCaseRole.Type1))
              {
                case "AP":
                  if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "AR":
                  if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "CH":
                  if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  if (!IsEmpty(local.DetermineChFv.Name))
                  {
                    local.Ch.Index = 0;

                    for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                      local.Ch.Index)
                    {
                      if (!local.Ch.CheckSize())
                      {
                        break;
                      }

                      if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator))
                      {
                        local.ChFv.FamilyViolenceIndicator =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        local.FieldValue.Value =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        UseSpCabCreateUpdateFieldValue3();

                        if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                        {
                          ExitState = "ACO_NN0000_ALL_OK";
                        }

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          export.ErrorDocumentField.ScreenPrompt =
                            "Creation Error";
                          export.ErrorFieldValue.Value = "Field:  " + local
                            .Temp.Name;

                          return;
                        }

                        local.FieldValue.Value =
                          Spaces(FieldValue.Value_MaxLength);
                        ++import.ExpImpRowLockFieldValue.Count;

                        break;
                      }
                    }

                    local.Ch.CheckIndex();
                    local.DetermineChFv.Name = "";
                  }

                  break;
                case "PR":
                  if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                default:
                  break;
              }

              if (local.CsePerson.HomePhone.GetValueOrDefault() > 0)
              {
                local.SpPrintWorkSet.PhoneAreaCode =
                  local.CsePerson.HomePhoneAreaCode.GetValueOrDefault();
                local.SpPrintWorkSet.Phone7Digit =
                  local.CsePerson.HomePhone.GetValueOrDefault();
                local.SpPrintWorkSet.PhoneExt = "";
                local.FieldValue.Value = UseSpDocFormatPhoneNumber();
              }

              break;
            case "*LASTNM":
              local.FieldValue.Value = local.CsePersonsWorkSet.LastName;

              break;
            case "*LOCADDR1":
              // mjr
              // ------------------------------------------------------
              // 05/08/2001
              // WR# 291 - For locate request documents, find the address based 
              // on
              // the following rules:
              // 1)  Use the address of the identifier provided; else
              // 2)  Use a current verified address; else
              // 3)  Use an address provided by the Locate Agency; else
              // 4)  Use any address on the system.
              // -------------------------------------------------------------------
              local.ProcessGroup.Flag = "A";

              switch(TrimEnd(local.CurrentCaseRole.Type1))
              {
                case "AP":
                  if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "AR":
                  if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "CH":
                  if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  if (!IsEmpty(local.DetermineChFv.Name))
                  {
                    local.Ch.Index = 0;

                    for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                      local.Ch.Index)
                    {
                      if (!local.Ch.CheckSize())
                      {
                        break;
                      }

                      if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator))
                      {
                        local.ChFv.FamilyViolenceIndicator =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        local.FieldValue.Value =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        UseSpCabCreateUpdateFieldValue3();

                        if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                        {
                          ExitState = "ACO_NN0000_ALL_OK";
                        }

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          export.ErrorDocumentField.ScreenPrompt =
                            "Creation Error";
                          export.ErrorFieldValue.Value = "Field:  " + local
                            .Temp.Name;

                          return;
                        }

                        local.FieldValue.Value =
                          Spaces(FieldValue.Value_MaxLength);
                        ++import.ExpImpRowLockFieldValue.Count;

                        break;
                      }
                    }

                    local.Ch.CheckIndex();
                    local.DetermineChFv.Name = "";
                  }

                  break;
                case "PR":
                  if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                default:
                  break;
              }

              if (Lt(local.NullDateWorkArea.Timestamp,
                export.SpDocKey.KeyPersonAddress))
              {
                if (ReadCsePersonAddress())
                {
                  // mjr
                  // ------------------------------------------
                  // 05/08/2001
                  // Found address using identifier provided
                  // -------------------------------------------------------
                  MoveCsePersonAddress(entities.CsePersonAddress,
                    local.CsePersonAddress);
                }
              }

              if (IsEmpty(local.CsePersonAddress.LocationType))
              {
                UseSiGetCsePersonMailingAddr2();

                if (Lt(local.NullDateWorkArea.Date,
                  local.CsePersonAddress.VerifiedDate) && !
                  Lt(import.Current.Date, local.CsePersonAddress.VerifiedDate) &&
                  Lt(import.Current.Date, local.CsePersonAddress.EndDate))
                {
                  // mjr
                  // ------------------------------------------
                  // 05/08/2001
                  // Found current verified address
                  // -------------------------------------------------------
                }
                else
                {
                  // mjr
                  // ------------------------------------------
                  // 05/08/2001
                  // Either an address was found that was not current and
                  // verified, or we don't have an address on the system.
                  // Search for an address provided by the Locate Agency.
                  // If one is found, overwrite the address in local
                  // cse_person_address.  If one is not found, use whatever
                  // we have in local cse_person_address
                  // -------------------------------------------------------
                  if (ReadLocateRequest())
                  {
                    if (!IsEmpty(entities.LocateRequest.Street1))
                    {
                      // mjr
                      // ------------------------------------------
                      // 05/08/2001
                      // Found address provided by Locate Agency
                      // -------------------------------------------------------
                      if (IsEmpty(entities.LocateRequest.Province))
                      {
                        local.CsePersonAddress.LocationType = "D";
                      }
                      else
                      {
                        local.CsePersonAddress.LocationType = "F";
                      }

                      local.CsePersonAddress.Street1 =
                        entities.LocateRequest.Street1;
                      local.CsePersonAddress.Street2 =
                        entities.LocateRequest.Street2;
                      local.CsePersonAddress.Street3 =
                        entities.LocateRequest.Street3;
                      local.CsePersonAddress.Street4 =
                        entities.LocateRequest.Street4;
                      local.CsePersonAddress.City = entities.LocateRequest.City;
                      local.CsePersonAddress.State =
                        entities.LocateRequest.State;
                      local.CsePersonAddress.ZipCode =
                        entities.LocateRequest.ZipCode5;
                      local.CsePersonAddress.Zip4 =
                        entities.LocateRequest.ZipCode4;
                      local.CsePersonAddress.Zip3 =
                        entities.LocateRequest.ZipCode3;
                      local.CsePersonAddress.Country =
                        entities.LocateRequest.Country;
                      local.CsePersonAddress.PostalCode =
                        entities.LocateRequest.PostalCode;
                      local.CsePersonAddress.Province =
                        entities.LocateRequest.Province;
                    }
                  }
                }
              }

              if (!IsEmpty(local.CsePersonAddress.LocationType))
              {
                local.SpPrintWorkSet.LocationType =
                  local.CsePersonAddress.LocationType;
                local.SpPrintWorkSet.Street1 =
                  local.CsePersonAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  local.CsePersonAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 =
                  local.CsePersonAddress.Street3 ?? Spaces(25);
                local.SpPrintWorkSet.Street4 =
                  local.CsePersonAddress.Street4 ?? Spaces(25);
                local.SpPrintWorkSet.City = local.CsePersonAddress.City ?? Spaces
                  (15);
                local.SpPrintWorkSet.State = local.CsePersonAddress.State ?? Spaces
                  (2);
                local.SpPrintWorkSet.ZipCode =
                  local.CsePersonAddress.ZipCode ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 = local.CsePersonAddress.Zip4 ?? Spaces
                  (4);
                local.SpPrintWorkSet.Zip3 = local.CsePersonAddress.Zip3 ?? Spaces
                  (3);
                local.SpPrintWorkSet.Country =
                  local.CsePersonAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  local.CsePersonAddress.PostalCode ?? Spaces(10);
                local.SpPrintWorkSet.Province =
                  local.CsePersonAddress.Province ?? Spaces(5);
                UseSpDocFormatAddress();
              }

              break;
            case "*MIDINIT":
              local.FieldValue.Value = local.CsePersonsWorkSet.MiddleInitial;

              break;
            case "*MOCITY":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.City ?? "";

              break;
            case "*MODOB":
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              if (AsChar(local.CsePerson.Type1) != 'O')
              {
                local.DateWorkArea.Date =
                  local.NaturalParentCsePersonsWorkSet.Dob;
                UseSpDocFormatDate1();
              }

              break;
            case "*MOFRSTNM":
              local.FieldValue.Value =
                local.NaturalParentCsePersonsWorkSet.FirstName;

              break;
            case "*MOLASTNM":
              local.FieldValue.Value =
                local.NaturalParentCsePersonsWorkSet.LastName;

              break;
            case "*MOMIDDLI":
              local.FieldValue.Value =
                local.NaturalParentCsePersonsWorkSet.MiddleInitial;

              break;
            case "*MOSSN":
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              // 05/13/2009	J Huss		CQ# 11072	Corrected view being passed to 
              // sp_doc_format_ssn.
              UseSpDocFormatSsn4();

              break;
            case "*MOSSNXX":
              // mjr
              // --------------------------------------------------------------------------------
              // 03/04/2008
              // WR 276, 277	Added fields for masked SSNs  (*SSNXX, **SSNXX, *
              // MOSSNXX, **MOSSNXX)
              // ---------------------------------------------------------------------------------------------
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              UseSpDocFormatSsn2();

              break;
            case "*MOSTATE":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.State ?? "";

              break;
            case "*MOSTR1":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.Street1 ?? "";

              break;
            case "*MOSTR2":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.Street2 ?? "";

              break;
            case "*MOZIP":
              local.FieldValue.Value =
                local.NaturalParentCsePersonAddress.ZipCode ?? "";

              if (!IsEmpty(local.NaturalParentCsePersonAddress.Zip4))
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "-"
                  + (local.NaturalParentCsePersonAddress.Zip4 ?? "");

                if (!IsEmpty(local.NaturalParentCsePersonAddress.Zip3))
                {
                  local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "-"
                    + (local.NaturalParentCsePersonAddress.Zip3 ?? "");
                }
              }

              break;
            case "*MTHRNM":
              local.SpPrintWorkSet.FirstName =
                local.NaturalParentCsePersonsWorkSet.FirstName;
              local.SpPrintWorkSet.MidInitial =
                local.NaturalParentCsePersonsWorkSet.MiddleInitial;
              local.SpPrintWorkSet.LastName =
                local.NaturalParentCsePersonsWorkSet.LastName;
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            case "*NM":
              if (Equal(local.CsePersonsWorkSet.Number, "HEALTHINS") && IsEmpty
                (local.CsePersonsWorkSet.LastName))
              {
                local.FieldValue.Value = local.Contact.CompanyName ?? "";
              }
              else
              {
                local.SpPrintWorkSet.FirstName =
                  local.CsePersonsWorkSet.FirstName;
                local.SpPrintWorkSet.MidInitial =
                  local.CsePersonsWorkSet.MiddleInitial;
                local.SpPrintWorkSet.LastName =
                  local.CsePersonsWorkSet.LastName;
                local.FieldValue.Value = UseSpDocFormatName();
              }

              break;
            case "*NMORGZ":
              // mjr
              // ----------------------------------------------
              // 09/19/2001
              // PR# 124861 - Give the name if the CSE_Person is a person.
              // Otherwise, give the Organization name if the CSE_Person is
              // an organization.
              // -----------------------------------------------------------
              if (!IsEmpty(local.CsePersonsWorkSet.FirstName))
              {
                local.SpPrintWorkSet.FirstName =
                  local.CsePersonsWorkSet.FirstName;
                local.SpPrintWorkSet.MidInitial =
                  local.CsePersonsWorkSet.MiddleInitial;
                local.SpPrintWorkSet.LastName =
                  local.CsePersonsWorkSet.LastName;
                local.FieldValue.Value = UseSpDocFormatName();
              }
              else
              {
                local.FieldValue.Value = local.CsePersonsWorkSet.FormattedName;
              }

              break;
            case "*NUMBER":
              local.FieldValue.Value = local.CsePersonsWorkSet.Number;

              break;
            case "*POB":
              if (!IsEmpty(local.CsePerson.BirthPlaceState))
              {
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue = local.CsePerson.BirthPlaceState ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();

                if (!IsEmpty(local.CsePerson.BirthPlaceCity))
                {
                  local.FieldValue.Value =
                    TrimEnd(local.CsePerson.BirthPlaceCity) + ", " + local
                    .CodeValue.Description;
                }
                else
                {
                  local.FieldValue.Value = local.CodeValue.Description;
                }
              }
              else if (!IsEmpty(local.CsePerson.BirthplaceCountry))
              {
                local.Code.CodeName = "FIPS COUNTRY CODE";
                local.CodeValue.Cdvalue = local.CsePerson.BirthplaceCountry ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();

                if (!IsEmpty(local.CsePerson.BirthPlaceCity))
                {
                  local.FieldValue.Value =
                    TrimEnd(local.CsePerson.BirthPlaceCity) + ", " + local
                    .CodeValue.Description;
                }
                else
                {
                  local.FieldValue.Value = local.CodeValue.Description;
                }
              }
              else if (!IsEmpty(local.CsePerson.BirthPlaceCity))
              {
                local.FieldValue.Value = local.CsePerson.BirthPlaceCity ?? "";
              }
              else
              {
              }

              break;
            case "*RACE":
              if (!IsEmpty(local.CsePerson.Race))
              {
                local.Code.CodeName = "RACE";
                local.CodeValue.Cdvalue = local.CsePerson.Race ?? Spaces(10);
                UseCabGetCodeValueDescription();
                local.FieldValue.Value = local.CodeValue.Description;
              }

              break;
            case "*SEX":
              local.FieldValue.Value = local.CsePersonsWorkSet.Sex;

              break;
            case "*SSN":
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              UseSpDocFormatSsn3();

              break;
            case "*SSNXX":
              // mjr
              // --------------------------------------------------------------------------------
              // 03/04/2008
              // WR 276, 277	Added fields for masked SSNs  (*SSNXX, **SSNXX, *
              // MOSSNXX, **MOSSNXX)
              // ---------------------------------------------------------------------------------------------
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              UseSpDocFormatSsn1();

              break;
            case "*STATE":
              local.ProcessGroup.Flag = "";
              local.FieldValue.Value = local.CsePersonAddress.State ?? "";

              break;
            case "*STRT1":
              local.ProcessGroup.Flag = "";
              local.FieldValue.Value = local.CsePersonAddress.Street1 ?? "";

              break;
            case "*STRT2":
              local.ProcessGroup.Flag = "";
              local.FieldValue.Value = local.CsePersonAddress.Street2 ?? "";

              break;
            case "*WEIGHT":
              if (local.CsePerson.Weight.GetValueOrDefault() > 0)
              {
                local.BatchConvertNumToText.TextNumber15 =
                  NumberToString(local.CsePerson.Weight.GetValueOrDefault(), 15);
                  
                local.Position.Count =
                  Verify(local.BatchConvertNumToText.TextNumber15, "0");
                local.FieldValue.Value =
                  Substring(local.BatchConvertNumToText.TextNumber15,
                  BatchConvertNumToText.TextNumber15_MaxLength,
                  local.Position.Count, 16 - local.Position.Count);
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + " lbs.";
                  
              }

              break;
            case "*ZIP":
              local.ProcessGroup.Flag = "";
              local.FieldValue.Value = local.CsePersonAddress.ZipCode ?? "";

              if (!IsEmpty(local.CsePersonAddress.Zip4))
              {
                local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "-"
                  + (local.CsePersonAddress.Zip4 ?? "");

                if (!IsEmpty(local.CsePersonAddress.Zip3))
                {
                  local.FieldValue.Value = TrimEnd(local.FieldValue.Value) + "-"
                    + (local.CsePersonAddress.Zip3 ?? "");
                }
              }

              break;
            case "**ADDR1":
              UseSpDocFormatAddress();

              break;
            case "**AGE":
              local.ChildAge.Count = Year(import.Current.Date) - Year
                (local.CsePersonsWorkSet.Dob);

              if (Month(local.CsePersonsWorkSet.Dob) > Month
                (import.Current.Date))
              {
                --local.ChildAge.Count;
              }

              if (local.ChildAge.Count <= 0)
              {
                local.FieldValue.Value = "0";
              }
              else
              {
                local.FieldValue.Value =
                  NumberToString(local.ChildAge.Count,
                  Verify(NumberToString(local.ChildAge.Count, 15), "0"), 16 -
                  Verify(NumberToString(local.ChildAge.Count, 15), "0"));
              }

              break;
            case "**DOB":
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              if (AsChar(local.CsePerson.Type1) != 'O')
              {
                local.DateWorkArea.Date = local.CsePersonsWorkSet.Dob;
                UseSpDocFormatDate1();
              }

              break;
            case "**FAADDR1":
              local.ProcessGroup.Flag = "A";
              local.SpPrintWorkSet.LocationType =
                local.NaturalParentCsePersonAddress.LocationType;
              local.SpPrintWorkSet.Street1 =
                local.NaturalParentCsePersonAddress.Street1 ?? Spaces(25);
              local.SpPrintWorkSet.Street2 =
                local.NaturalParentCsePersonAddress.Street2 ?? Spaces(25);
              local.SpPrintWorkSet.Street3 =
                local.NaturalParentCsePersonAddress.Street3 ?? Spaces(25);
              local.SpPrintWorkSet.Street4 =
                local.NaturalParentCsePersonAddress.Street4 ?? Spaces(25);
              local.SpPrintWorkSet.City =
                local.NaturalParentCsePersonAddress.City ?? Spaces(15);
              local.SpPrintWorkSet.State =
                local.NaturalParentCsePersonAddress.State ?? Spaces(2);
              local.SpPrintWorkSet.ZipCode =
                local.NaturalParentCsePersonAddress.ZipCode ?? Spaces(5);
              local.SpPrintWorkSet.Zip4 =
                local.NaturalParentCsePersonAddress.Zip4 ?? Spaces(4);
              local.SpPrintWorkSet.Zip3 =
                local.NaturalParentCsePersonAddress.Zip3 ?? Spaces(3);
              local.SpPrintWorkSet.Country =
                local.NaturalParentCsePersonAddress.Country ?? Spaces(2);
              local.SpPrintWorkSet.PostalCode =
                local.NaturalParentCsePersonAddress.PostalCode ?? Spaces(10);
              local.SpPrintWorkSet.Province =
                local.NaturalParentCsePersonAddress.Province ?? Spaces(5);
              UseSpDocFormatAddress();

              break;
            case "**FADOB":
              if (AsChar(local.CsePerson.Type1) != 'O')
              {
                local.DateWorkArea.Date =
                  local.NaturalParentCsePersonsWorkSet.Dob;
                UseSpDocFormatDate1();
              }

              break;
            case "**FAHOPHO":
              switch(TrimEnd(local.CurrentCaseRole.Type1))
              {
                case "AP":
                  if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "AR":
                  if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "CH":
                  if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  if (!IsEmpty(local.DetermineChFv.Name))
                  {
                    local.Ch.Index = 0;

                    for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                      local.Ch.Index)
                    {
                      if (!local.Ch.CheckSize())
                      {
                        break;
                      }

                      if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator))
                      {
                        local.ChFv.FamilyViolenceIndicator =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        local.FieldValue.Value =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        UseSpCabCreateUpdateFieldValue3();

                        if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                        {
                          ExitState = "ACO_NN0000_ALL_OK";
                        }

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          export.ErrorDocumentField.ScreenPrompt =
                            "Creation Error";
                          export.ErrorFieldValue.Value = "Field:  " + local
                            .Temp.Name;

                          return;
                        }

                        local.FieldValue.Value =
                          Spaces(FieldValue.Value_MaxLength);
                        ++import.ExpImpRowLockFieldValue.Count;

                        break;
                      }
                    }

                    local.Ch.CheckIndex();
                    local.DetermineChFv.Name = "";
                  }

                  break;
                case "PR":
                  if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                default:
                  break;
              }

              if (Lt(0, entities.NaturalParentCsePerson.HomePhone))
              {
                local.SpPrintWorkSet.PhoneAreaCode =
                  entities.NaturalParentCsePerson.HomePhoneAreaCode.
                    GetValueOrDefault();
                local.SpPrintWorkSet.Phone7Digit =
                  entities.NaturalParentCsePerson.HomePhone.GetValueOrDefault();
                  
                local.SpPrintWorkSet.PhoneExt = "";
                local.FieldValue.Value = UseSpDocFormatPhoneNumber();
              }

              break;
            case "**FAPOB":
              if (!IsEmpty(entities.NaturalParentCsePerson.BirthPlaceState))
              {
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue =
                  entities.NaturalParentCsePerson.BirthPlaceState ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();

                if (!IsEmpty(entities.NaturalParentCsePerson.BirthPlaceCity))
                {
                  local.FieldValue.Value =
                    TrimEnd(entities.NaturalParentCsePerson.BirthPlaceCity) + ", " +
                    local.CodeValue.Description;
                }
                else
                {
                  local.FieldValue.Value = local.CodeValue.Description;
                }
              }
              else if (!IsEmpty(entities.NaturalParentCsePerson.
                BirthplaceCountry))
              {
                local.Code.CodeName = "FIPS COUNTRY CODE";
                local.CodeValue.Cdvalue =
                  entities.NaturalParentCsePerson.BirthplaceCountry ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();

                if (!IsEmpty(entities.NaturalParentCsePerson.BirthPlaceCity))
                {
                  local.FieldValue.Value =
                    TrimEnd(entities.NaturalParentCsePerson.BirthPlaceCity) + ", " +
                    local.CodeValue.Description;
                }
                else
                {
                  local.FieldValue.Value = local.CodeValue.Description;
                }
              }
              else if (!IsEmpty(entities.NaturalParentCsePerson.BirthPlaceCity))
              {
                local.FieldValue.Value =
                  entities.NaturalParentCsePerson.BirthPlaceCity;
              }
              else
              {
              }

              break;
            case "**FASSN":
              UseSpDocFormatSsn4();

              break;
            case "**FASSNXX":
              UseSpDocFormatSsn2();

              break;
            case "**FTHRNM":
              local.SpPrintWorkSet.FirstName =
                local.NaturalParentCsePersonsWorkSet.FirstName;
              local.SpPrintWorkSet.MidInitial =
                local.NaturalParentCsePersonsWorkSet.MiddleInitial;
              local.SpPrintWorkSet.LastName =
                local.NaturalParentCsePersonsWorkSet.LastName;
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            case "**HOMEPH":
              // mjr
              // --------------------------------------------------------------------------------
              // 04/11/2008
              // WR 302941	Added fields for DET2PATP (Multiple APs Home Phone, 
              // Sex, SSN, DOB)
              // All already existed except **HOMEPH, which was copied from *
              // HOMEPH
              // ---------------------------------------------------------------------------------------------
              switch(TrimEnd(local.CurrentCaseRole.Type1))
              {
                case "AP":
                  if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "AR":
                  if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "CH":
                  if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  if (!IsEmpty(local.DetermineChFv.Name))
                  {
                    local.Ch.Index = 0;

                    for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                      local.Ch.Index)
                    {
                      if (!local.Ch.CheckSize())
                      {
                        break;
                      }

                      if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator))
                      {
                        local.ChFv.FamilyViolenceIndicator =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        local.FieldValue.Value =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        UseSpCabCreateUpdateFieldValue3();

                        if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                        {
                          ExitState = "ACO_NN0000_ALL_OK";
                        }

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          export.ErrorDocumentField.ScreenPrompt =
                            "Creation Error";
                          export.ErrorFieldValue.Value = "Field:  " + local
                            .Temp.Name;

                          return;
                        }

                        local.FieldValue.Value =
                          Spaces(FieldValue.Value_MaxLength);
                        ++import.ExpImpRowLockFieldValue.Count;

                        break;
                      }
                    }

                    local.Ch.CheckIndex();
                    local.DetermineChFv.Name = "";
                  }

                  break;
                case "PR":
                  if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                default:
                  break;
              }

              if (local.CsePerson.HomePhone.GetValueOrDefault() > 0)
              {
                local.SpPrintWorkSet.PhoneAreaCode =
                  local.CsePerson.HomePhoneAreaCode.GetValueOrDefault();
                local.SpPrintWorkSet.Phone7Digit =
                  local.CsePerson.HomePhone.GetValueOrDefault();
                local.SpPrintWorkSet.PhoneExt = "";
                local.FieldValue.Value = UseSpDocFormatPhoneNumber();
              }

              break;
            case "**MOADDR1":
              local.ProcessGroup.Flag = "A";
              local.SpPrintWorkSet.LocationType =
                local.NaturalParentCsePersonAddress.LocationType;
              local.SpPrintWorkSet.Street1 =
                local.NaturalParentCsePersonAddress.Street1 ?? Spaces(25);
              local.SpPrintWorkSet.Street2 =
                local.NaturalParentCsePersonAddress.Street2 ?? Spaces(25);
              local.SpPrintWorkSet.Street3 =
                local.NaturalParentCsePersonAddress.Street3 ?? Spaces(25);
              local.SpPrintWorkSet.Street4 =
                local.NaturalParentCsePersonAddress.Street4 ?? Spaces(25);
              local.SpPrintWorkSet.City =
                local.NaturalParentCsePersonAddress.City ?? Spaces(15);
              local.SpPrintWorkSet.State =
                local.NaturalParentCsePersonAddress.State ?? Spaces(2);
              local.SpPrintWorkSet.ZipCode =
                local.NaturalParentCsePersonAddress.ZipCode ?? Spaces(5);
              local.SpPrintWorkSet.Zip4 =
                local.NaturalParentCsePersonAddress.Zip4 ?? Spaces(4);
              local.SpPrintWorkSet.Zip3 =
                local.NaturalParentCsePersonAddress.Zip3 ?? Spaces(3);
              local.SpPrintWorkSet.Country =
                local.NaturalParentCsePersonAddress.Country ?? Spaces(2);
              local.SpPrintWorkSet.PostalCode =
                local.NaturalParentCsePersonAddress.PostalCode ?? Spaces(10);
              local.SpPrintWorkSet.Province =
                local.NaturalParentCsePersonAddress.Province ?? Spaces(5);
              UseSpDocFormatAddress();

              break;
            case "**MODOB":
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              if (AsChar(local.CsePerson.Type1) != 'O')
              {
                local.DateWorkArea.Date =
                  local.NaturalParentCsePersonsWorkSet.Dob;
                UseSpDocFormatDate1();
              }

              break;
            case "**MOHOPHO":
              switch(TrimEnd(local.CurrentCaseRole.Type1))
              {
                case "AP":
                  if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "AR":
                  if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                case "CH":
                  if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  if (!IsEmpty(local.DetermineChFv.Name))
                  {
                    local.Ch.Index = 0;

                    for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                      local.Ch.Index)
                    {
                      if (!local.Ch.CheckSize())
                      {
                        break;
                      }

                      if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator))
                      {
                        local.ChFv.FamilyViolenceIndicator =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        local.FieldValue.Value =
                          local.Ch.Item.GlocalChCsePerson.
                            FamilyViolenceIndicator ?? "";
                        UseSpCabCreateUpdateFieldValue3();

                        if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                        {
                          ExitState = "ACO_NN0000_ALL_OK";
                        }

                        if (!IsExitState("ACO_NN0000_ALL_OK"))
                        {
                          export.ErrorDocumentField.ScreenPrompt =
                            "Creation Error";
                          export.ErrorFieldValue.Value = "Field:  " + local
                            .Temp.Name;

                          return;
                        }

                        local.FieldValue.Value =
                          Spaces(FieldValue.Value_MaxLength);
                        ++import.ExpImpRowLockFieldValue.Count;

                        break;
                      }
                    }

                    local.Ch.CheckIndex();
                    local.DetermineChFv.Name = "";
                  }

                  break;
                case "PR":
                  if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
                  {
                    goto Test5;
                  }

                  break;
                default:
                  break;
              }

              if (Lt(0, entities.NaturalParentCsePerson.HomePhone))
              {
                local.SpPrintWorkSet.PhoneAreaCode =
                  entities.NaturalParentCsePerson.HomePhoneAreaCode.
                    GetValueOrDefault();
                local.SpPrintWorkSet.Phone7Digit =
                  entities.NaturalParentCsePerson.HomePhone.GetValueOrDefault();
                  
                local.SpPrintWorkSet.PhoneExt = "";
                local.FieldValue.Value = UseSpDocFormatPhoneNumber();
              }

              break;
            case "**MOPOB":
              if (!IsEmpty(entities.NaturalParentCsePerson.BirthPlaceState))
              {
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue =
                  entities.NaturalParentCsePerson.BirthPlaceState ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();

                if (!IsEmpty(entities.NaturalParentCsePerson.BirthPlaceCity))
                {
                  local.FieldValue.Value =
                    TrimEnd(entities.NaturalParentCsePerson.BirthPlaceCity) + ", " +
                    local.CodeValue.Description;
                }
                else
                {
                  local.FieldValue.Value = local.CodeValue.Description;
                }
              }
              else if (!IsEmpty(entities.NaturalParentCsePerson.
                BirthplaceCountry))
              {
                local.Code.CodeName = "FIPS COUNTRY CODE";
                local.CodeValue.Cdvalue =
                  entities.NaturalParentCsePerson.BirthplaceCountry ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();

                if (!IsEmpty(entities.NaturalParentCsePerson.BirthPlaceCity))
                {
                  local.FieldValue.Value =
                    TrimEnd(entities.NaturalParentCsePerson.BirthPlaceCity) + ", " +
                    local.CodeValue.Description;
                }
                else
                {
                  local.FieldValue.Value = local.CodeValue.Description;
                }
              }
              else if (!IsEmpty(entities.NaturalParentCsePerson.BirthPlaceCity))
              {
                local.FieldValue.Value =
                  entities.NaturalParentCsePerson.BirthPlaceCity;
              }
              else
              {
              }

              break;
            case "**MOSSN":
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              // 05/13/2009	J Huss		CQ# 11072	Corrected view being passed to 
              // sp_doc_format_ssn.
              UseSpDocFormatSsn4();

              break;
            case "**MOSSNXX":
              // mjr
              // --------------------------------------------------------------------------------
              // 03/04/2008
              // WR 276, 277	Added fields for masked SSNs  (*SSNXX, **SSNXX, *
              // MOSSNXX, **MOSSNXX)
              // ---------------------------------------------------------------------------------------------
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              UseSpDocFormatSsn2();

              break;
            case "**MTHRNM":
              local.SpPrintWorkSet.FirstName =
                local.NaturalParentCsePersonsWorkSet.FirstName;
              local.SpPrintWorkSet.MidInitial =
                local.NaturalParentCsePersonsWorkSet.MiddleInitial;
              local.SpPrintWorkSet.LastName =
                local.NaturalParentCsePersonsWorkSet.LastName;
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            case "**NM":
              local.SpPrintWorkSet.FirstName =
                local.CsePersonsWorkSet.FirstName;
              local.SpPrintWorkSet.MidInitial =
                local.CsePersonsWorkSet.MiddleInitial;
              local.SpPrintWorkSet.LastName = local.CsePersonsWorkSet.LastName;
              local.FieldValue.Value = UseSpDocFormatName();

              break;
            case "**FIRSTNM":
              local.FieldValue.Value = local.CsePersonsWorkSet.FirstName;

              break;
            case "**MIDINIT":
              local.FieldValue.Value = local.CsePersonsWorkSet.MiddleInitial;

              break;
            case "**LASTNM":
              local.FieldValue.Value = local.CsePersonsWorkSet.LastName;

              break;
            case "**POB":
              if (!IsEmpty(local.CsePerson.BirthPlaceState))
              {
                local.Code.CodeName = "STATE CODE";
                local.CodeValue.Cdvalue = local.CsePerson.BirthPlaceState ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();

                if (!IsEmpty(local.CsePerson.BirthPlaceCity))
                {
                  local.FieldValue.Value =
                    TrimEnd(local.CsePerson.BirthPlaceCity) + ", " + local
                    .CodeValue.Description;
                }
                else
                {
                  local.FieldValue.Value = local.CodeValue.Description;
                }
              }
              else if (!IsEmpty(local.CsePerson.BirthplaceCountry))
              {
                local.Code.CodeName = "FIPS COUNTRY CODE";
                local.CodeValue.Cdvalue = local.CsePerson.BirthplaceCountry ?? Spaces
                  (10);
                UseCabGetCodeValueDescription();

                if (!IsEmpty(local.CsePerson.BirthPlaceCity))
                {
                  local.FieldValue.Value =
                    TrimEnd(local.CsePerson.BirthPlaceCity) + ", " + local
                    .CodeValue.Description;
                }
                else
                {
                  local.FieldValue.Value = local.CodeValue.Description;
                }
              }
              else if (!IsEmpty(local.CsePerson.BirthPlaceCity))
              {
                local.FieldValue.Value = local.CsePerson.BirthPlaceCity ?? "";
              }
              else
              {
              }

              break;
            case "**SEX":
              local.FieldValue.Value = local.CsePersonsWorkSet.Sex;

              break;
            case "**SSN":
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              UseSpDocFormatSsn3();

              break;
            case "**SSNXX":
              // mjr
              // --------------------------------------------------------------------------------
              // 03/04/2008
              // WR 276, 277	Added fields for masked SSNs  (*SSNXX, **SSNXX, *
              // MOSSNXX, **MOSSNXX)
              // ---------------------------------------------------------------------------------------------
              // 04/08/2009	J Huss		CQ# 10302	Added support for unknown SSN and 
              // DOB placeholders.
              UseSpDocFormatSsn1();

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        case "WRKRCOMP":
          switch(TrimEnd(local.CurrentCaseRole.Type1))
          {
            case "AP":
              if (!IsEmpty(local.ApFv.FamilyViolenceIndicator))
              {
                goto Test5;
              }

              break;
            case "AR":
              if (!IsEmpty(local.ArFv.FamilyViolenceIndicator))
              {
                goto Test5;
              }

              break;
            case "CH":
              if (!IsEmpty(local.ChFv.FamilyViolenceIndicator))
              {
                goto Test5;
              }

              if (!IsEmpty(local.DetermineChFv.Name))
              {
                local.Ch.Index = 0;

                for(var limit = local.Ch.Count; local.Ch.Index < limit; ++
                  local.Ch.Index)
                {
                  if (!local.Ch.CheckSize())
                  {
                    break;
                  }

                  if (!IsEmpty(local.Ch.Item.GlocalChCsePerson.
                    FamilyViolenceIndicator))
                  {
                    local.ChFv.FamilyViolenceIndicator =
                      local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator ?? "";
                    local.FieldValue.Value =
                      local.Ch.Item.GlocalChCsePerson.
                        FamilyViolenceIndicator ?? "";
                    UseSpCabCreateUpdateFieldValue3();

                    if (IsExitState("DOCUMENT_FIELD_NF_RB"))
                    {
                      ExitState = "ACO_NN0000_ALL_OK";
                    }

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      export.ErrorDocumentField.ScreenPrompt = "Creation Error";
                      export.ErrorFieldValue.Value = "Field:  " + local
                        .Temp.Name;

                      return;
                    }

                    local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
                    ++import.ExpImpRowLockFieldValue.Count;

                    break;
                  }
                }

                local.Ch.CheckIndex();
                local.DetermineChFv.Name = "";
              }

              break;
            case "PR":
              if (!IsEmpty(local.PrFv.FamilyViolenceIndicator))
              {
                goto Test5;
              }

              break;
            default:
              break;
          }

          if (!Lt(local.NullDateWorkArea.Timestamp,
            export.SpDocKey.KeyWorkerComp))
          {
            if (ReadOtherIncomeSource1())
            {
              export.SpDocKey.KeyWorkerComp =
                entities.OtherIncomeSource.Identifier;
            }
          }
          else if (ReadOtherIncomeSource2())
          {
            export.SpDocKey.KeyWorkerComp =
              entities.OtherIncomeSource.Identifier;
          }

          if (!Lt(local.NullDateWorkArea.Timestamp,
            export.SpDocKey.KeyWorkerComp))
          {
            local.PreviousField.Name = local.CurrentCaseRole.Type1 + "WRKZ";

            continue;
          }

          switch(TrimEnd(local.CurrentField.Name))
          {
            case "*WKRCMPA1":
              local.ProcessGroup.Flag = "A";

              if (ReadNonEmployIncomeSourceAddress1())
              {
                local.SpPrintWorkSet.LocationType =
                  entities.NonEmployIncomeSourceAddress.LocationType;
                local.SpPrintWorkSet.Street1 =
                  entities.NonEmployIncomeSourceAddress.Street1 ?? Spaces(25);
                local.SpPrintWorkSet.Street2 =
                  entities.NonEmployIncomeSourceAddress.Street2 ?? Spaces(25);
                local.SpPrintWorkSet.Street3 =
                  entities.NonEmployIncomeSourceAddress.Street3 ?? Spaces(25);
                local.SpPrintWorkSet.Street4 =
                  entities.NonEmployIncomeSourceAddress.Street4 ?? Spaces(25);
                local.SpPrintWorkSet.City =
                  entities.NonEmployIncomeSourceAddress.City ?? Spaces(15);
                local.SpPrintWorkSet.State =
                  entities.NonEmployIncomeSourceAddress.State ?? Spaces(2);
                local.SpPrintWorkSet.ZipCode =
                  entities.NonEmployIncomeSourceAddress.ZipCode ?? Spaces(5);
                local.SpPrintWorkSet.Zip4 =
                  entities.NonEmployIncomeSourceAddress.Zip4 ?? Spaces(4);
                local.SpPrintWorkSet.Zip3 =
                  entities.NonEmployIncomeSourceAddress.Zip3 ?? Spaces(3);
                local.SpPrintWorkSet.Country =
                  entities.NonEmployIncomeSourceAddress.Country ?? Spaces(2);
                local.SpPrintWorkSet.PostalCode =
                  entities.NonEmployIncomeSourceAddress.PostalCode ?? Spaces
                  (10);
                local.SpPrintWorkSet.Province =
                  entities.NonEmployIncomeSourceAddress.Province ?? Spaces(5);
                UseSpDocFormatAddress();
              }

              break;
            case "*WKRCMPNM":
              local.FieldValue.Value = entities.OtherIncomeSource.Name;

              break;
            default:
              export.ErrorDocumentField.ScreenPrompt = "Invalid Field";
              export.ErrorFieldValue.Value = "Field:  " + TrimEnd
                (entities.Field.Name) + ",  Subroutine:  " + entities
                .Field.SubroutineName;

              break;
          }

          break;
        default:
          export.ErrorDocumentField.ScreenPrompt = "Invalid Subroutine";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Subroutine:  " + entities
            .Field.SubroutineName;

          break;
      }

Test5:

      if (!IsEmpty(export.ErrorDocumentField.ScreenPrompt) || !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsEmpty(export.ErrorDocumentField.ScreenPrompt))
        {
          export.ErrorDocumentField.ScreenPrompt = "Processing Error";
          export.ErrorFieldValue.Value = "Field:  " + TrimEnd
            (entities.Field.Name) + ",  Person:  " + local
            .CsePersonsWorkSet.Number;
        }

        return;
      }

      if (AsChar(local.ProcessGroup.Flag) == 'A')
      {
        // mjr
        // ----------------------------------------------
        // Field is an address
        //    Process 1-5 of group_local
        // -------------------------------------------------
        local.Position.Count = Length(TrimEnd(entities.Field.Name));
        local.CurrentNumberField.Name =
          Substring(entities.Field.Name, 1, local.Position.Count - 1);

        for(local.Address.Index = 0; local.Address.Index < local.Address.Count; ++
          local.Address.Index)
        {
          if (!local.Address.CheckSize())
          {
            break;
          }

          // mjr---> Increment Field Name
          local.Temp.Name = NumberToString(local.Address.Index + 1, 10);
          local.Position.Count = Verify(local.Temp.Name, "0");
          local.Temp.Name =
            Substring(local.Temp.Name, local.Position.Count, 16 -
            local.Position.Count);
          local.Temp.Name = TrimEnd(local.CurrentNumberField.Name) + local
            .Temp.Name;

          if (local.Address.Index == 2)
          {
            if (Equal(local.Address.Item.GlocalAddress.Value, ","))
            {
              local.Address.Update.GlocalAddress.Value =
                Spaces(FieldValue.Value_MaxLength);
            }
          }

          local.FieldValue.Value = local.Address.Item.GlocalAddress.Value ?? "";
          UseSpCabCreateUpdateFieldValue2();

          if (IsExitState("DOCUMENT_FIELD_NF_RB"))
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ErrorDocumentField.ScreenPrompt = "Creation Error";
            export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

            return;
          }

          local.Address.Update.GlocalAddress.Value =
            Spaces(FieldValue.Value_MaxLength);
          ++import.ExpImpRowLockFieldValue.Count;
        }

        local.Address.CheckIndex();
      }
      else
      {
        // mjr
        // ----------------------------------------------
        // Field is a single value
        //    Process local field_value
        // -------------------------------------------------
        // --------------------------------------------------------
        // Add Field_value
        // Need to update PAD to include doc_field
        // (which means document and field need to be imported)
        // --------------------------------------------------------
        UseSpCabCreateUpdateFieldValue1();

        if (IsExitState("DOCUMENT_FIELD_NF_RB"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.ErrorDocumentField.ScreenPrompt = "Creation Error";
          export.ErrorFieldValue.Value = "Field:  " + entities.Field.Name;

          return;
        }

        ++import.ExpImpRowLockFieldValue.Count;
      }

      // mjr
      // -----------------------------------------------------------
      // set Previous Field to skip the rest of the group, if applicable.
      // --------------------------------------------------------------
      switch(TrimEnd(local.CurrentField.Name))
      {
        case "* LA1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + " LA5";
            

          break;
        case "* LA2":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + " LA5";
            

          break;
        case "* LA3":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + " LA5";
            

          break;
        case "* LA4":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + " LA5";
            

          break;
        case "*EMP LA1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "EMP LA3";
            

          break;
        case "*EMP LA2":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "EMP LA3";
            

          break;
        case "*ATADD1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "ATADD5"
            ;

          break;
        case "*ADDR1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "ADDR5"
            ;

          break;
        case "*EMPAD1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "EMPAD5"
            ;

          break;
        case "*EMPHQAD1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "EMPHQAD5"
            ;

          break;
        case "*INCSADD1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "INCSADD5"
            ;

          break;
        case "*LOCADDR1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "LOCADDR5"
            ;

          break;
        case "*RSDNTAD1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "RSDNTAD5"
            ;

          break;
        case "*WKRCMPA1":
          local.PreviousField.Name = TrimEnd(local.CurrentCaseRole.Type1) + "WKRCMPA5"
            ;

          break;
        default:
          if (local.CurrentNumberCommon.Count >= 0 && local
            .CurrentNumberCommon.Count <= 9)
          {
            local.CurrentField.Name = "**" + Substring
              (entities.Field.Name, Field.Name_MaxLength, 4, 7);

            switch(TrimEnd(local.CurrentField.Name))
            {
              case "**ADDR1":
                local.PreviousField.Name =
                  TrimEnd(local.CurrentCaseRole.Type1) + NumberToString
                  (local.CurrentNumberCommon.Count, 15, 1) + "ADDR5";

                break;
              case "**EMAD1":
                local.PreviousField.Name =
                  TrimEnd(local.CurrentCaseRole.Type1) + NumberToString
                  (local.CurrentNumberCommon.Count, 15, 1) + "EMAD5";

                break;
              case "**MOADDR1":
                local.PreviousField.Name =
                  TrimEnd(local.CurrentCaseRole.Type1) + NumberToString
                  (local.CurrentNumberCommon.Count, 15, 1) + "MOADDR5";
                local.GetPersonNameFlag.Flag = "Y";

                break;
              case "**FAADDR1":
                local.PreviousField.Name =
                  TrimEnd(local.CurrentCaseRole.Type1) + NumberToString
                  (local.CurrentNumberCommon.Count, 15, 1) + "FAADDR5";
                local.GetPersonNameFlag.Flag = "Y";

                break;
              case "**FTHRNM":
                local.GetPersonNameFlag.Flag = "Y";

                break;
              default:
                break;
            }
          }

          break;
      }
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveChAlternateMethods(Local.
    ChAlternateMethodsGroup source,
    SpDocGetPerson.Import.AlternateMethodsGroup target)
  {
    target.G.Code = source.GlocalCh.Code;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveExport1ToAddress(SpDocFormatAddress.Export.
    ExportGroup source, Local.AddressGroup target)
  {
    target.GlocalAddress.Value = source.G.Value;
  }

  private static void MoveExport1ToAp(SpDocGetPerson.Export.ExportGroup source,
    Local.ApGroup target)
  {
    target.GlocalApCsePersonsWorkSet.Assign(source.GcsePersonsWorkSet);
    target.GlocalApCsePerson.Assign(source.GcsePerson);
  }

  private static void MoveExport1ToCh(SpDocGetPerson.Export.ExportGroup source,
    Local.ChGroup target)
  {
    target.GlocalChCsePersonsWorkSet.Assign(source.GcsePersonsWorkSet);
    target.GlocalChCsePerson.Assign(source.GcsePerson);
  }

  private static void MoveExport1ToNaturalParent(SpDocGetPerson.Export.
    ExportGroup source, Local.NaturalParentGroup target)
  {
    target.GlocalNaturalParentCsePersonsWorkSet.
      Assign(source.GcsePersonsWorkSet);
    target.GlocalNaturalParentCsePerson.Assign(source.GcsePerson);
  }

  private static void MoveField(Field source, Field target)
  {
    target.Name = source.Name;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveFieldValue1(FieldValue source, FieldValue target)
  {
    target.Value = source.Value;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveFieldValue2(FieldValue source, FieldValue target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveSpPrintWorkSet1(SpPrintWorkSet source,
    SpPrintWorkSet target)
  {
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneExt = source.PhoneExt;
    target.Phone7Digit = source.Phone7Digit;
  }

  private static void MoveSpPrintWorkSet2(SpPrintWorkSet source,
    SpPrintWorkSet target)
  {
    target.LastName = source.LastName;
    target.FirstName = source.FirstName;
    target.MidInitial = source.MidInitial;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseSiGetCsePersonMailingAddr1()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = entities.NaturalParentCsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    local.NaturalParentCsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSiGetCsePersonMailingAddr2()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSpCabCreateUpdateFieldValue1()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Field.Name = entities.Field.Name;
    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue2()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = local.Temp.Name;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpCabCreateUpdateFieldValue3()
  {
    var useImport = new SpCabCreateUpdateFieldValue.Import();
    var useExport = new SpCabCreateUpdateFieldValue.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Field.Name = local.DetermineChFv.Name;
    MoveFieldValue1(local.FieldValue, useImport.FieldValue);

    Call(SpCabCreateUpdateFieldValue.Execute, useImport, useExport);
  }

  private void UseSpDocFormatAddress()
  {
    var useImport = new SpDocFormatAddress.Import();
    var useExport = new SpDocFormatAddress.Export();

    useImport.SpPrintWorkSet.Assign(local.SpPrintWorkSet);

    Call(SpDocFormatAddress.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Address, MoveExport1ToAddress);
  }

  private void UseSpDocFormatDate1()
  {
    var useImport = new SpDocFormatDate.Import();
    var useExport = new SpDocFormatDate.Export();

    useImport.PopulatePlaceholder.Flag = local.PopulatePlaceholder.Flag;
    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(SpDocFormatDate.Execute, useImport, useExport);

    local.FieldValue.Value = useExport.FieldValue.Value;
  }

  private string UseSpDocFormatDate2()
  {
    var useImport = new SpDocFormatDate.Import();
    var useExport = new SpDocFormatDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(SpDocFormatDate.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatName()
  {
    var useImport = new SpDocFormatName.Import();
    var useExport = new SpDocFormatName.Export();

    MoveSpPrintWorkSet2(local.SpPrintWorkSet, useImport.SpPrintWorkSet);

    Call(SpDocFormatName.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private string UseSpDocFormatPhoneNumber()
  {
    var useImport = new SpDocFormatPhoneNumber.Import();
    var useExport = new SpDocFormatPhoneNumber.Export();

    MoveSpPrintWorkSet1(local.SpPrintWorkSet, useImport.SpPrintWorkSet);

    Call(SpDocFormatPhoneNumber.Execute, useImport, useExport);

    return useExport.FieldValue.Value ?? "";
  }

  private void UseSpDocFormatSsn1()
  {
    var useImport = new SpDocFormatSsn.Import();
    var useExport = new SpDocFormatSsn.Export();

    useImport.MaskSsn.Flag = local.MaskSsn.Flag;
    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    useImport.PopulatePlaceholder.Flag = local.PopulatePlaceholder.Flag;
    MoveCsePerson(local.CsePerson, useImport.CsePerson);

    Call(SpDocFormatSsn.Execute, useImport, useExport);

    local.FieldValue.Value = useExport.Ssn.Value;
  }

  private void UseSpDocFormatSsn2()
  {
    var useImport = new SpDocFormatSsn.Import();
    var useExport = new SpDocFormatSsn.Export();

    useImport.PopulatePlaceholder.Flag = local.PopulatePlaceholder.Flag;
    useImport.MaskSsn.Flag = local.MaskSsn.Flag;
    MoveCsePerson(local.CsePerson, useImport.CsePerson);
    useImport.CsePersonsWorkSet.Ssn = local.NaturalParentCsePersonsWorkSet.Ssn;

    Call(SpDocFormatSsn.Execute, useImport, useExport);

    local.FieldValue.Value = useExport.Ssn.Value;
  }

  private void UseSpDocFormatSsn3()
  {
    var useImport = new SpDocFormatSsn.Import();
    var useExport = new SpDocFormatSsn.Export();

    useImport.PopulatePlaceholder.Flag = local.PopulatePlaceholder.Flag;
    MoveCsePerson(local.CsePerson, useImport.CsePerson);
    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;

    Call(SpDocFormatSsn.Execute, useImport, useExport);

    local.FieldValue.Value = useExport.Ssn.Value;
  }

  private void UseSpDocFormatSsn4()
  {
    var useImport = new SpDocFormatSsn.Import();
    var useExport = new SpDocFormatSsn.Export();

    useImport.PopulatePlaceholder.Flag = local.PopulatePlaceholder.Flag;
    MoveCsePerson(local.CsePerson, useImport.CsePerson);
    useImport.CsePersonsWorkSet.Ssn = local.NaturalParentCsePersonsWorkSet.Ssn;

    Call(SpDocFormatSsn.Execute, useImport, useExport);

    local.FieldValue.Value = useExport.Ssn.Value;
  }

  private void UseSpDocGetPerson1()
  {
    var useImport = new SpDocGetPerson.Import();
    var useExport = new SpDocGetPerson.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.Document.BusinessObject = import.Document.BusinessObject;
    useImport.Current.Date = import.Current.Date;
    useImport.ReturnGroup.Flag = local.NeedGroup.Flag;
    useImport.AlternateMethod.Flag = local.AlternateMethod.Flag;
    useImport.CaseRole.Type1 = local.CurrentCaseRole.Type1;
    useImport.SpDocKey.Assign(export.SpDocKey);

    Call(SpDocGetPerson.Execute, useImport, useExport);

    local.CsePerson.Assign(useExport.CsePerson);
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
    export.SpDocKey.Assign(useExport.SpDocKey);
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    local.Contact.CompanyName = useExport.Contact.CompanyName;
  }

  private void UseSpDocGetPerson2()
  {
    var useImport = new SpDocGetPerson.Import();
    var useExport = new SpDocGetPerson.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.Document.BusinessObject = import.Document.BusinessObject;
    useImport.Current.Date = import.Current.Date;
    local.ChAlternateMethods.CopyTo(
      useImport.AlternateMethods, MoveChAlternateMethods);
    useImport.ReturnGroup.Flag = local.NeedGroup.Flag;
    useImport.CaseRole.Type1 = local.CurrentCaseRole.Type1;
    useImport.SpDocKey.Assign(export.SpDocKey);

    Call(SpDocGetPerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Ch, MoveExport1ToCh);
    export.SpDocKey.Assign(useExport.SpDocKey);
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
  }

  private void UseSpDocGetPerson3()
  {
    var useImport = new SpDocGetPerson.Import();
    var useExport = new SpDocGetPerson.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.Document.BusinessObject = import.Document.BusinessObject;
    useImport.Current.Date = import.Current.Date;
    useImport.ReturnGroup.Flag = local.NeedGroup.Flag;
    useImport.CaseRole.Type1 = local.CurrentCaseRole.Type1;
    useImport.SpDocKey.Assign(export.SpDocKey);

    Call(SpDocGetPerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Ap, MoveExport1ToAp);
    export.SpDocKey.Assign(useExport.SpDocKey);
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
  }

  private void UseSpDocGetPerson4()
  {
    var useImport = new SpDocGetPerson.Import();
    var useExport = new SpDocGetPerson.Export();

    useImport.Batch.Flag = import.Batch.Flag;
    useImport.Document.BusinessObject = import.Document.BusinessObject;
    useImport.Current.Date = import.Current.Date;
    useImport.CaseRole.Type1 = local.NaturalParentCaseRole.Type1;
    useImport.SpDocKey.Assign(export.SpDocKey);

    Call(SpDocGetPerson.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.NaturalParent, MoveExport1ToNaturalParent);
    local.NaturalParentCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.SpDocKey.Assign(useExport.SpDocKey);
    export.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    export.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
  }

  private bool ReadAdminAppealAppellantAddress()
  {
    entities.AdminAppealAppellantAddress.Populated = false;

    return Read("ReadAdminAppealAppellantAddress",
      (db, command) =>
      {
        db.SetInt32(command, "aapIdentifier", export.SpDocKey.KeyAdminAppeal);
      },
      (db, reader) =>
      {
        entities.AdminAppealAppellantAddress.AapIdentifier =
          db.GetInt32(reader, 0);
        entities.AdminAppealAppellantAddress.Type1 = db.GetString(reader, 1);
        entities.AdminAppealAppellantAddress.Street1 = db.GetString(reader, 2);
        entities.AdminAppealAppellantAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.AdminAppealAppellantAddress.City = db.GetString(reader, 4);
        entities.AdminAppealAppellantAddress.StateProvince =
          db.GetString(reader, 5);
        entities.AdminAppealAppellantAddress.Country =
          db.GetNullableString(reader, 6);
        entities.AdminAppealAppellantAddress.PostalCode =
          db.GetNullableString(reader, 7);
        entities.AdminAppealAppellantAddress.ZipCode = db.GetString(reader, 8);
        entities.AdminAppealAppellantAddress.Zip4 =
          db.GetNullableString(reader, 9);
        entities.AdminAppealAppellantAddress.Zip3 =
          db.GetNullableString(reader, 10);
        entities.AdminAppealAppellantAddress.Populated = true;
      });
  }

  private bool ReadContactAddress()
  {
    entities.ContactAddress.Populated = false;

    return Read("ReadContactAddress",
      (db, command) =>
      {
        db.
          SetInt64(command, "identifier", export.SpDocKey.KeyHealthInsCoverage);
          
      },
      (db, reader) =>
      {
        entities.ContactAddress.ConNumber = db.GetInt32(reader, 0);
        entities.ContactAddress.CspNumber = db.GetString(reader, 1);
        entities.ContactAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.ContactAddress.Street1 = db.GetNullableString(reader, 3);
        entities.ContactAddress.Street2 = db.GetNullableString(reader, 4);
        entities.ContactAddress.City = db.GetNullableString(reader, 5);
        entities.ContactAddress.State = db.GetNullableString(reader, 6);
        entities.ContactAddress.Province = db.GetNullableString(reader, 7);
        entities.ContactAddress.PostalCode = db.GetNullableString(reader, 8);
        entities.ContactAddress.ZipCode5 = db.GetNullableString(reader, 9);
        entities.ContactAddress.ZipCode4 = db.GetNullableString(reader, 10);
        entities.ContactAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.ContactAddress.Country = db.GetNullableString(reader, 12);
        entities.ContactAddress.AddressType = db.GetNullableString(reader, 13);
        entities.ContactAddress.Populated = true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.SpDocKey.KeyPersonAddress.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 14);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonCaseRole1()
  {
    entities.NaturalParentCsePerson.Populated = false;
    entities.NaturalParentCaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "type", local.NaturalParentCaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", local.Ch.Item.GlocalChCsePersonsWorkSet.Number);
          
      },
      (db, reader) =>
      {
        entities.NaturalParentCsePerson.Number = db.GetString(reader, 0);
        entities.NaturalParentCaseRole.CspNumber = db.GetString(reader, 0);
        entities.NaturalParentCsePerson.Type1 = db.GetString(reader, 1);
        entities.NaturalParentCsePerson.BirthPlaceState =
          db.GetNullableString(reader, 2);
        entities.NaturalParentCsePerson.HomePhone =
          db.GetNullableInt32(reader, 3);
        entities.NaturalParentCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 4);
        entities.NaturalParentCsePerson.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 5);
        entities.NaturalParentCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 6);
        entities.NaturalParentCsePerson.BirthplaceCountry =
          db.GetNullableString(reader, 7);
        entities.NaturalParentCaseRole.CasNumber = db.GetString(reader, 8);
        entities.NaturalParentCaseRole.Type1 = db.GetString(reader, 9);
        entities.NaturalParentCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.NaturalParentCaseRole.StartDate =
          db.GetNullableDate(reader, 11);
        entities.NaturalParentCaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.NaturalParentCsePerson.Populated = true;
        entities.NaturalParentCaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.NaturalParentCsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.NaturalParentCaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole2()
  {
    entities.NaturalParentCsePerson.Populated = false;
    entities.NaturalParentCaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "type", local.NaturalParentCaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", local.Ch.Item.GlocalChCsePersonsWorkSet.Number);
          
      },
      (db, reader) =>
      {
        entities.NaturalParentCsePerson.Number = db.GetString(reader, 0);
        entities.NaturalParentCaseRole.CspNumber = db.GetString(reader, 0);
        entities.NaturalParentCsePerson.Type1 = db.GetString(reader, 1);
        entities.NaturalParentCsePerson.BirthPlaceState =
          db.GetNullableString(reader, 2);
        entities.NaturalParentCsePerson.HomePhone =
          db.GetNullableInt32(reader, 3);
        entities.NaturalParentCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 4);
        entities.NaturalParentCsePerson.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 5);
        entities.NaturalParentCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 6);
        entities.NaturalParentCsePerson.BirthplaceCountry =
          db.GetNullableString(reader, 7);
        entities.NaturalParentCaseRole.CasNumber = db.GetString(reader, 8);
        entities.NaturalParentCaseRole.Type1 = db.GetString(reader, 9);
        entities.NaturalParentCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.NaturalParentCaseRole.StartDate =
          db.GetNullableDate(reader, 11);
        entities.NaturalParentCaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.NaturalParentCsePerson.Populated = true;
        entities.NaturalParentCaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.NaturalParentCsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.NaturalParentCaseRole.Type1);
      });
  }

  private bool ReadCsePersonCaseRole3()
  {
    entities.NaturalParentCsePerson.Populated = false;
    entities.NaturalParentCaseRole.Populated = false;

    return Read("ReadCsePersonCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "type", local.NaturalParentCaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.NaturalParentCsePerson.Number = db.GetString(reader, 0);
        entities.NaturalParentCaseRole.CspNumber = db.GetString(reader, 0);
        entities.NaturalParentCsePerson.Type1 = db.GetString(reader, 1);
        entities.NaturalParentCsePerson.BirthPlaceState =
          db.GetNullableString(reader, 2);
        entities.NaturalParentCsePerson.HomePhone =
          db.GetNullableInt32(reader, 3);
        entities.NaturalParentCsePerson.BirthPlaceCity =
          db.GetNullableString(reader, 4);
        entities.NaturalParentCsePerson.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 5);
        entities.NaturalParentCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 6);
        entities.NaturalParentCsePerson.BirthplaceCountry =
          db.GetNullableString(reader, 7);
        entities.NaturalParentCaseRole.CasNumber = db.GetString(reader, 8);
        entities.NaturalParentCaseRole.Type1 = db.GetString(reader, 9);
        entities.NaturalParentCaseRole.Identifier = db.GetInt32(reader, 10);
        entities.NaturalParentCaseRole.StartDate =
          db.GetNullableDate(reader, 11);
        entities.NaturalParentCaseRole.EndDate = db.GetNullableDate(reader, 12);
        entities.NaturalParentCsePerson.Populated = true;
        entities.NaturalParentCaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.NaturalParentCsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.NaturalParentCaseRole.Type1);
      });
  }

  private bool ReadEmployer1()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.SpDocKey.KeyIncomeSource.GetValueOrDefault());
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 3);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 4);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          local.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.Name = db.GetNullableString(reader, 2);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 3);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 4);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployerAddress1()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.SpDocKey.KeyIncomeSource.GetValueOrDefault());
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Province = db.GetNullableString(reader, 7);
        entities.EmployerAddress.Country = db.GetNullableString(reader, 8);
        entities.EmployerAddress.PostalCode = db.GetNullableString(reader, 9);
        entities.EmployerAddress.State = db.GetNullableString(reader, 10);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 11);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 12);
        entities.EmployerAddress.Zip3 = db.GetNullableString(reader, 13);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 14);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private bool ReadEmployerAddress2()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          local.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Province = db.GetNullableString(reader, 7);
        entities.EmployerAddress.Country = db.GetNullableString(reader, 8);
        entities.EmployerAddress.PostalCode = db.GetNullableString(reader, 9);
        entities.EmployerAddress.State = db.GetNullableString(reader, 10);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 11);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 12);
        entities.EmployerAddress.Zip3 = db.GetNullableString(reader, 13);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 14);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private bool ReadEmployerAddress3()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress3",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Province = db.GetNullableString(reader, 7);
        entities.EmployerAddress.Country = db.GetNullableString(reader, 8);
        entities.EmployerAddress.PostalCode = db.GetNullableString(reader, 9);
        entities.EmployerAddress.State = db.GetNullableString(reader, 10);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 11);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 12);
        entities.EmployerAddress.Zip3 = db.GetNullableString(reader, 13);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 14);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private bool ReadEmployerAddress4()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress4",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Hq.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 1);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 2);
        entities.EmployerAddress.City = db.GetNullableString(reader, 3);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 4);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 6);
        entities.EmployerAddress.Province = db.GetNullableString(reader, 7);
        entities.EmployerAddress.Country = db.GetNullableString(reader, 8);
        entities.EmployerAddress.PostalCode = db.GetNullableString(reader, 9);
        entities.EmployerAddress.State = db.GetNullableString(reader, 10);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 11);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 12);
        entities.EmployerAddress.Zip3 = db.GetNullableString(reader, 13);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 14);
        entities.EmployerAddress.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private bool ReadEmployerEmployerRelation()
  {
    entities.Hq.Populated = false;
    entities.EmployerRelation.Populated = false;

    return Read("ReadEmployerEmployerRelation",
      (db, command) =>
      {
        db.SetInt32(command, "empLocId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Hq.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 0);
        entities.Hq.Name = db.GetNullableString(reader, 1);
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 2);
        entities.EmployerRelation.VerifiedDate = db.GetNullableDate(reader, 3);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 4);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 5);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 6);
        entities.EmployerRelation.Type1 = db.GetString(reader, 7);
        entities.Hq.Populated = true;
        entities.EmployerRelation.Populated = true;
      });
  }

  private bool ReadEmployerRegisteredAgent()
  {
    entities.EmployerRegisteredAgent.Populated = false;

    return Read("ReadEmployerRegisteredAgent",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "effectiveDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerRegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.EmployerRegisteredAgent.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.EmployerRegisteredAgent.EndDate =
          db.GetNullableDate(reader, 2);
        entities.EmployerRegisteredAgent.RaaId = db.GetString(reader, 3);
        entities.EmployerRegisteredAgent.EmpId = db.GetInt32(reader, 4);
        entities.EmployerRegisteredAgent.Populated = true;
      });
  }

  private IEnumerable<bool> ReadField()
  {
    entities.Field.Populated = false;

    return ReadEach("ReadField",
      (db, command) =>
      {
        db.SetString(command, "docName", import.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          import.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "dependancy", import.Field.Dependancy);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.Populated = true;

        return true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadIncomeSource1()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "endDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.Code = db.GetNullableString(reader, 5);
        entities.IncomeSource.CspINumber = db.GetString(reader, 6);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 7);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 8);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 10);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadIncomeSource2()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.Code = db.GetNullableString(reader, 5);
        entities.IncomeSource.CspINumber = db.GetString(reader, 6);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 7);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 8);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 10);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadIncomeSource3()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource3",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDt", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.Code = db.GetNullableString(reader, 5);
        entities.IncomeSource.CspINumber = db.GetString(reader, 6);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 7);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 8);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 10);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadIncomeSource4()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.SpDocKey.KeyIncomeSource.GetValueOrDefault());
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.Code = db.GetNullableString(reader, 5);
        entities.IncomeSource.CspINumber = db.GetString(reader, 6);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 7);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 8);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 10);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadIncomeSource5()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource5",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.SpDocKey.KeyIncomeSource.GetValueOrDefault());
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.Name = db.GetNullableString(reader, 4);
        entities.IncomeSource.Code = db.GetNullableString(reader, 5);
        entities.IncomeSource.CspINumber = db.GetString(reader, 6);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 7);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 8);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 10);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadInformationRequest()
  {
    entities.InformationRequest.Populated = false;

    return Read("ReadInformationRequest",
      (db, command) =>
      {
        db.SetInt64(command, "numb", export.SpDocKey.KeyInfoRequest);
      },
      (db, reader) =>
      {
        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.ApplicantLastName =
          db.GetNullableString(reader, 1);
        entities.InformationRequest.ApplicantFirstName =
          db.GetNullableString(reader, 2);
        entities.InformationRequest.ApplicantMiddleInitial =
          db.GetNullableString(reader, 3);
        entities.InformationRequest.ApplicantStreet1 =
          db.GetNullableString(reader, 4);
        entities.InformationRequest.ApplicantStreet2 =
          db.GetNullableString(reader, 5);
        entities.InformationRequest.ApplicantCity =
          db.GetNullableString(reader, 6);
        entities.InformationRequest.ApplicantState =
          db.GetNullableString(reader, 7);
        entities.InformationRequest.ApplicantZip5 =
          db.GetNullableString(reader, 8);
        entities.InformationRequest.ApplicantZip4 =
          db.GetNullableString(reader, 9);
        entities.InformationRequest.ApplicantZip3 =
          db.GetNullableString(reader, 10);
        entities.InformationRequest.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
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
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
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
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 4);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 6);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 7);
        entities.IncomeSource.Type1 = db.GetString(reader, 8);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 10);
        entities.IncomeSource.Name = db.GetNullableString(reader, 11);
        entities.IncomeSource.Code = db.GetNullableString(reader, 12);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 13);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 14);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 15);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 16);
        entities.IncomeSource.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadLegalActionIncomeSourceIncomeSource2()
  {
    entities.IncomeSource.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSourceIncomeSource2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
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
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 4);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 6);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 7);
        entities.IncomeSource.Type1 = db.GetString(reader, 8);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 10);
        entities.IncomeSource.Name = db.GetNullableString(reader, 11);
        entities.IncomeSource.Code = db.GetNullableString(reader, 12);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 13);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 14);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 15);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 16);
        entities.IncomeSource.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadLegalActionIncomeSourceIncomeSource3()
  {
    entities.IncomeSource.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadLegalActionIncomeSourceIncomeSource3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", import.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
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
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 4);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 5);
        entities.LegalActionIncomeSource.OrderType =
          db.GetNullableString(reader, 6);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 7);
        entities.IncomeSource.Type1 = db.GetString(reader, 8);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 9);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 10);
        entities.IncomeSource.Name = db.GetNullableString(reader, 11);
        entities.IncomeSource.Code = db.GetNullableString(reader, 12);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 13);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 14);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 15);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 16);
        entities.IncomeSource.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadLocateRequest()
  {
    entities.LocateRequest.Populated = false;

    return Read("ReadLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "csePersonNumber", local.CsePerson.Number);
        db.SetString(
          command, "agencyNumber", export.SpDocKey.KeyLocateRequestAgency);
        db.SetInt32(
          command, "sequenceNumber", export.SpDocKey.KeyLocateRequestSource);
      },
      (db, reader) =>
      {
        entities.LocateRequest.CsePersonNumber = db.GetString(reader, 0);
        entities.LocateRequest.AgencyNumber = db.GetString(reader, 1);
        entities.LocateRequest.SequenceNumber = db.GetInt32(reader, 2);
        entities.LocateRequest.Street1 = db.GetNullableString(reader, 3);
        entities.LocateRequest.AddressType = db.GetNullableString(reader, 4);
        entities.LocateRequest.Street2 = db.GetNullableString(reader, 5);
        entities.LocateRequest.Street3 = db.GetNullableString(reader, 6);
        entities.LocateRequest.Street4 = db.GetNullableString(reader, 7);
        entities.LocateRequest.City = db.GetNullableString(reader, 8);
        entities.LocateRequest.State = db.GetNullableString(reader, 9);
        entities.LocateRequest.ZipCode5 = db.GetNullableString(reader, 10);
        entities.LocateRequest.ZipCode4 = db.GetNullableString(reader, 11);
        entities.LocateRequest.ZipCode3 = db.GetNullableString(reader, 12);
        entities.LocateRequest.Province = db.GetNullableString(reader, 13);
        entities.LocateRequest.PostalCode = db.GetNullableString(reader, 14);
        entities.LocateRequest.Country = db.GetNullableString(reader, 15);
        entities.LocateRequest.Populated = true;
      });
  }

  private bool ReadNonEmployIncomeSourceAddress1()
  {
    entities.NonEmployIncomeSourceAddress.Populated = false;

    return Read("ReadNonEmployIncomeSourceAddress1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "isrIdentifier",
          export.SpDocKey.KeyWorkerComp.GetValueOrDefault());
        db.SetString(command, "csePerson", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.NonEmployIncomeSourceAddress.IsrIdentifier =
          db.GetDateTime(reader, 0);
        entities.NonEmployIncomeSourceAddress.Street1 =
          db.GetNullableString(reader, 1);
        entities.NonEmployIncomeSourceAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.NonEmployIncomeSourceAddress.City =
          db.GetNullableString(reader, 3);
        entities.NonEmployIncomeSourceAddress.State =
          db.GetNullableString(reader, 4);
        entities.NonEmployIncomeSourceAddress.ZipCode =
          db.GetNullableString(reader, 5);
        entities.NonEmployIncomeSourceAddress.Zip4 =
          db.GetNullableString(reader, 6);
        entities.NonEmployIncomeSourceAddress.Zip3 =
          db.GetNullableString(reader, 7);
        entities.NonEmployIncomeSourceAddress.Street3 =
          db.GetNullableString(reader, 8);
        entities.NonEmployIncomeSourceAddress.Street4 =
          db.GetNullableString(reader, 9);
        entities.NonEmployIncomeSourceAddress.Province =
          db.GetNullableString(reader, 10);
        entities.NonEmployIncomeSourceAddress.PostalCode =
          db.GetNullableString(reader, 11);
        entities.NonEmployIncomeSourceAddress.Country =
          db.GetNullableString(reader, 12);
        entities.NonEmployIncomeSourceAddress.LocationType =
          db.GetString(reader, 13);
        entities.NonEmployIncomeSourceAddress.CsePerson =
          db.GetString(reader, 14);
        entities.NonEmployIncomeSourceAddress.Populated = true;
        CheckValid<NonEmployIncomeSourceAddress>("LocationType",
          entities.NonEmployIncomeSourceAddress.LocationType);
      });
  }

  private bool ReadNonEmployIncomeSourceAddress2()
  {
    entities.NonEmployIncomeSourceAddress.Populated = false;

    return Read("ReadNonEmployIncomeSourceAddress2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "isrIdentifier",
          export.SpDocKey.KeyIncomeSource.GetValueOrDefault());
        db.SetString(command, "csePerson", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.NonEmployIncomeSourceAddress.IsrIdentifier =
          db.GetDateTime(reader, 0);
        entities.NonEmployIncomeSourceAddress.Street1 =
          db.GetNullableString(reader, 1);
        entities.NonEmployIncomeSourceAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.NonEmployIncomeSourceAddress.City =
          db.GetNullableString(reader, 3);
        entities.NonEmployIncomeSourceAddress.State =
          db.GetNullableString(reader, 4);
        entities.NonEmployIncomeSourceAddress.ZipCode =
          db.GetNullableString(reader, 5);
        entities.NonEmployIncomeSourceAddress.Zip4 =
          db.GetNullableString(reader, 6);
        entities.NonEmployIncomeSourceAddress.Zip3 =
          db.GetNullableString(reader, 7);
        entities.NonEmployIncomeSourceAddress.Street3 =
          db.GetNullableString(reader, 8);
        entities.NonEmployIncomeSourceAddress.Street4 =
          db.GetNullableString(reader, 9);
        entities.NonEmployIncomeSourceAddress.Province =
          db.GetNullableString(reader, 10);
        entities.NonEmployIncomeSourceAddress.PostalCode =
          db.GetNullableString(reader, 11);
        entities.NonEmployIncomeSourceAddress.Country =
          db.GetNullableString(reader, 12);
        entities.NonEmployIncomeSourceAddress.LocationType =
          db.GetString(reader, 13);
        entities.NonEmployIncomeSourceAddress.CsePerson =
          db.GetString(reader, 14);
        entities.NonEmployIncomeSourceAddress.Populated = true;
        CheckValid<NonEmployIncomeSourceAddress>("LocationType",
          entities.NonEmployIncomeSourceAddress.LocationType);
      });
  }

  private bool ReadOtherIncomeSource1()
  {
    entities.OtherIncomeSource.Populated = false;

    return Read("ReadOtherIncomeSource1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.OtherIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.OtherIncomeSource.Type1 = db.GetString(reader, 1);
        entities.OtherIncomeSource.Name = db.GetNullableString(reader, 2);
        entities.OtherIncomeSource.Code = db.GetNullableString(reader, 3);
        entities.OtherIncomeSource.CspINumber = db.GetString(reader, 4);
        entities.OtherIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.OtherIncomeSource.Type1);
      });
  }

  private bool ReadOtherIncomeSource2()
  {
    entities.OtherIncomeSource.Populated = false;

    return Read("ReadOtherIncomeSource2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          export.SpDocKey.KeyWorkerComp.GetValueOrDefault());
        db.SetString(command, "cspINumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.OtherIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.OtherIncomeSource.Type1 = db.GetString(reader, 1);
        entities.OtherIncomeSource.Name = db.GetNullableString(reader, 2);
        entities.OtherIncomeSource.Code = db.GetNullableString(reader, 3);
        entities.OtherIncomeSource.CspINumber = db.GetString(reader, 4);
        entities.OtherIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.OtherIncomeSource.Type1);
      });
  }

  private bool ReadPersonPrivateAttorney1()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney1",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", import.SpDocKey.KeyCase);
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
        db.SetDate(
          command, "dateRetained", import.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "dateDismissed",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "courtCaseNumber", local.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "countyAbbreviation", local.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", local.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.PersonPrivateAttorney.DateRetained = db.GetDate(reader, 3);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 4);
        entities.PersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.PersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.PersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.PersonPrivateAttorney.Phone = db.GetNullableInt32(reader, 9);
        entities.PersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.PersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 11);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 12);
        entities.PersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 13);
        entities.PersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 14);
        entities.PersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 15);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPersonPrivateAttorney2()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.Ch.Item.GlocalChCsePersonsWorkSet.Number);
          
        db.SetDate(
          command, "dateRetained", import.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "dateDismissed",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "courtCaseNumber", local.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "countyAbbreviation", local.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", local.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.PersonPrivateAttorney.DateRetained = db.GetDate(reader, 3);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 4);
        entities.PersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.PersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.PersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.PersonPrivateAttorney.Phone = db.GetNullableInt32(reader, 9);
        entities.PersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.PersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 11);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 12);
        entities.PersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 13);
        entities.PersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 14);
        entities.PersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 15);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPersonPrivateAttorney3()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
        db.SetDate(
          command, "dateRetained", import.Current.Date.GetValueOrDefault());
        db.SetDate(
          command, "dateDismissed",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "courtCaseNumber", local.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "countyAbbreviation", local.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", local.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.PersonPrivateAttorney.DateRetained = db.GetDate(reader, 3);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 4);
        entities.PersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.PersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.PersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.PersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.PersonPrivateAttorney.Phone = db.GetNullableInt32(reader, 9);
        entities.PersonPrivateAttorney.PhoneAreaCode =
          db.GetNullableInt32(reader, 10);
        entities.PersonPrivateAttorney.PhoneExt =
          db.GetNullableString(reader, 11);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 12);
        entities.PersonPrivateAttorney.FipsStateAbbreviation =
          db.GetNullableString(reader, 13);
        entities.PersonPrivateAttorney.FipsCountyAbbreviation =
          db.GetNullableString(reader, 14);
        entities.PersonPrivateAttorney.TribCountry =
          db.GetNullableString(reader, 15);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPrivateAttorneyAddress()
  {
    entities.PrivateAttorneyAddress.Populated = false;

    return Read("ReadPrivateAttorneyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "ppaIdentifier", local.PersonPrivateAttorney.Identifier);
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.PrivateAttorneyAddress.PpaIdentifier = db.GetInt32(reader, 0);
        entities.PrivateAttorneyAddress.CspNumber = db.GetString(reader, 1);
        entities.PrivateAttorneyAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.PrivateAttorneyAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.PrivateAttorneyAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.PrivateAttorneyAddress.City = db.GetNullableString(reader, 5);
        entities.PrivateAttorneyAddress.State = db.GetNullableString(reader, 6);
        entities.PrivateAttorneyAddress.Province =
          db.GetNullableString(reader, 7);
        entities.PrivateAttorneyAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.PrivateAttorneyAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.PrivateAttorneyAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.PrivateAttorneyAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.PrivateAttorneyAddress.Country =
          db.GetNullableString(reader, 12);
        entities.PrivateAttorneyAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.PrivateAttorneyAddress.Populated = true;
      });
  }

  private bool ReadRegisteredAgent()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerRegisteredAgent.Populated);
    entities.RegisteredAgent.Populated = false;

    return Read("ReadRegisteredAgent",
      (db, command) =>
      {
        db.SetString(
          command, "identifier", entities.EmployerRegisteredAgent.RaaId);
      },
      (db, reader) =>
      {
        entities.RegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.RegisteredAgent.Name = db.GetNullableString(reader, 1);
        entities.RegisteredAgent.Populated = true;
      });
  }

  private bool ReadRegisteredAgentAddress()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerRegisteredAgent.Populated);
    entities.RegisteredAgentAddress.Populated = false;

    return Read("ReadRegisteredAgentAddress",
      (db, command) =>
      {
        db.SetString(command, "ragId", entities.EmployerRegisteredAgent.RaaId);
      },
      (db, reader) =>
      {
        entities.RegisteredAgentAddress.Identifier = db.GetString(reader, 0);
        entities.RegisteredAgentAddress.Street1 =
          db.GetNullableString(reader, 1);
        entities.RegisteredAgentAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.RegisteredAgentAddress.City = db.GetNullableString(reader, 3);
        entities.RegisteredAgentAddress.State = db.GetNullableString(reader, 4);
        entities.RegisteredAgentAddress.ZipCode5 =
          db.GetNullableString(reader, 5);
        entities.RegisteredAgentAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.RegisteredAgentAddress.Zip3 = db.GetNullableString(reader, 7);
        entities.RegisteredAgentAddress.RagId = db.GetString(reader, 8);
        entities.RegisteredAgentAddress.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Populated = true;
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of ExpImpRowLockFieldValue.
    /// </summary>
    [JsonPropertyName("expImpRowLockFieldValue")]
    public Common ExpImpRowLockFieldValue
    {
      get => expImpRowLockFieldValue ??= new();
      set => expImpRowLockFieldValue = value;
    }

    private NextTranInfo nextTranInfo;
    private Standard standard;
    private SpDocKey spDocKey;
    private Common batch;
    private FieldValue fieldValue;
    private Document document;
    private Field field;
    private Infrastructure infrastructure;
    private DateWorkArea current;
    private Common expImpRowLockFieldValue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    /// <summary>
    /// A value of ZdelExportErrorInd.
    /// </summary>
    [JsonPropertyName("zdelExportErrorInd")]
    public Common ZdelExportErrorInd
    {
      get => zdelExportErrorInd ??= new();
      set => zdelExportErrorInd = value;
    }

    private SpDocKey spDocKey;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
    private Common zdelExportErrorInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A NaturalParentGroup group.</summary>
    [Serializable]
    public class NaturalParentGroup
    {
      /// <summary>
      /// A value of GlocalNaturalParentCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("glocalNaturalParentCsePersonsWorkSet")]
      public CsePersonsWorkSet GlocalNaturalParentCsePersonsWorkSet
      {
        get => glocalNaturalParentCsePersonsWorkSet ??= new();
        set => glocalNaturalParentCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GlocalNaturalParentCsePerson.
      /// </summary>
      [JsonPropertyName("glocalNaturalParentCsePerson")]
      public CsePerson GlocalNaturalParentCsePerson
      {
        get => glocalNaturalParentCsePerson ??= new();
        set => glocalNaturalParentCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet glocalNaturalParentCsePersonsWorkSet;
      private CsePerson glocalNaturalParentCsePerson;
    }

    /// <summary>A ChAlternateMethodsGroup group.</summary>
    [Serializable]
    public class ChAlternateMethodsGroup
    {
      /// <summary>
      /// A value of GlocalCh.
      /// </summary>
      [JsonPropertyName("glocalCh")]
      public ObligationType GlocalCh
      {
        get => glocalCh ??= new();
        set => glocalCh = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType glocalCh;
    }

    /// <summary>A ApAlternateMethodsGroup group.</summary>
    [Serializable]
    public class ApAlternateMethodsGroup
    {
      /// <summary>
      /// A value of GlocalAp.
      /// </summary>
      [JsonPropertyName("glocalAp")]
      public ObligationType GlocalAp
      {
        get => glocalAp ??= new();
        set => glocalAp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType glocalAp;
    }

    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of GlocalApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("glocalApCsePersonsWorkSet")]
      public CsePersonsWorkSet GlocalApCsePersonsWorkSet
      {
        get => glocalApCsePersonsWorkSet ??= new();
        set => glocalApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GlocalApCsePerson.
      /// </summary>
      [JsonPropertyName("glocalApCsePerson")]
      public CsePerson GlocalApCsePerson
      {
        get => glocalApCsePerson ??= new();
        set => glocalApCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet glocalApCsePersonsWorkSet;
      private CsePerson glocalApCsePerson;
    }

    /// <summary>A ChGroup group.</summary>
    [Serializable]
    public class ChGroup
    {
      /// <summary>
      /// A value of GlocalChCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("glocalChCsePersonsWorkSet")]
      public CsePersonsWorkSet GlocalChCsePersonsWorkSet
      {
        get => glocalChCsePersonsWorkSet ??= new();
        set => glocalChCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GlocalChCsePerson.
      /// </summary>
      [JsonPropertyName("glocalChCsePerson")]
      public CsePerson GlocalChCsePerson
      {
        get => glocalChCsePerson ??= new();
        set => glocalChCsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet glocalChCsePersonsWorkSet;
      private CsePerson glocalChCsePerson;
    }

    /// <summary>A AddressGroup group.</summary>
    [Serializable]
    public class AddressGroup
    {
      /// <summary>
      /// A value of GlocalAddress.
      /// </summary>
      [JsonPropertyName("glocalAddress")]
      public FieldValue GlocalAddress
      {
        get => glocalAddress ??= new();
        set => glocalAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue glocalAddress;
    }

    /// <summary>
    /// Gets a value of NaturalParent.
    /// </summary>
    [JsonIgnore]
    public Array<NaturalParentGroup> NaturalParent => naturalParent ??= new(
      NaturalParentGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of NaturalParent for json serialization.
    /// </summary>
    [JsonPropertyName("naturalParent")]
    [Computed]
    public IList<NaturalParentGroup> NaturalParent_Json
    {
      get => naturalParent;
      set => NaturalParent.Assign(value);
    }

    /// <summary>
    /// A value of NaturalParentCaseRole.
    /// </summary>
    [JsonPropertyName("naturalParentCaseRole")]
    public CaseRole NaturalParentCaseRole
    {
      get => naturalParentCaseRole ??= new();
      set => naturalParentCaseRole = value;
    }

    /// <summary>
    /// A value of PopulatePlaceholder.
    /// </summary>
    [JsonPropertyName("populatePlaceholder")]
    public Common PopulatePlaceholder
    {
      get => populatePlaceholder ??= new();
      set => populatePlaceholder = value;
    }

    /// <summary>
    /// A value of MaskSsn.
    /// </summary>
    [JsonPropertyName("maskSsn")]
    public Common MaskSsn
    {
      get => maskSsn ??= new();
      set => maskSsn = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of DetermineChFv.
    /// </summary>
    [JsonPropertyName("determineChFv")]
    public Field DetermineChFv
    {
      get => determineChFv ??= new();
      set => determineChFv = value;
    }

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
    /// A value of PrFv.
    /// </summary>
    [JsonPropertyName("prFv")]
    public CsePerson PrFv
    {
      get => prFv ??= new();
      set => prFv = value;
    }

    /// <summary>
    /// A value of ChFv.
    /// </summary>
    [JsonPropertyName("chFv")]
    public CsePerson ChFv
    {
      get => chFv ??= new();
      set => chFv = value;
    }

    /// <summary>
    /// A value of ArFv.
    /// </summary>
    [JsonPropertyName("arFv")]
    public CsePerson ArFv
    {
      get => arFv ??= new();
      set => arFv = value;
    }

    /// <summary>
    /// A value of ApFv.
    /// </summary>
    [JsonPropertyName("apFv")]
    public CsePerson ApFv
    {
      get => apFv ??= new();
      set => apFv = value;
    }

    /// <summary>
    /// Gets a value of ChAlternateMethods.
    /// </summary>
    [JsonIgnore]
    public Array<ChAlternateMethodsGroup> ChAlternateMethods =>
      chAlternateMethods ??= new(ChAlternateMethodsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ChAlternateMethods for json serialization.
    /// </summary>
    [JsonPropertyName("chAlternateMethods")]
    [Computed]
    public IList<ChAlternateMethodsGroup> ChAlternateMethods_Json
    {
      get => chAlternateMethods;
      set => ChAlternateMethods.Assign(value);
    }

    /// <summary>
    /// Gets a value of ApAlternateMethods.
    /// </summary>
    [JsonIgnore]
    public Array<ApAlternateMethodsGroup> ApAlternateMethods =>
      apAlternateMethods ??= new(ApAlternateMethodsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ApAlternateMethods for json serialization.
    /// </summary>
    [JsonPropertyName("apAlternateMethods")]
    [Computed]
    public IList<ApAlternateMethodsGroup> ApAlternateMethods_Json
    {
      get => apAlternateMethods;
      set => ApAlternateMethods.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
    }

    /// <summary>
    /// A value of SkipCh.
    /// </summary>
    [JsonPropertyName("skipCh")]
    public Common SkipCh
    {
      get => skipCh ??= new();
      set => skipCh = value;
    }

    /// <summary>
    /// A value of SkipAp.
    /// </summary>
    [JsonPropertyName("skipAp")]
    public Common SkipAp
    {
      get => skipAp ??= new();
      set => skipAp = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of ZdelLocalNaturalFather.
    /// </summary>
    [JsonPropertyName("zdelLocalNaturalFather")]
    public CsePersonsWorkSet ZdelLocalNaturalFather
    {
      get => zdelLocalNaturalFather ??= new();
      set => zdelLocalNaturalFather = value;
    }

    /// <summary>
    /// Gets a value of Ch.
    /// </summary>
    [JsonIgnore]
    public Array<ChGroup> Ch => ch ??= new(ChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ch for json serialization.
    /// </summary>
    [JsonPropertyName("ch")]
    [Computed]
    public IList<ChGroup> Ch_Json
    {
      get => ch;
      set => Ch.Assign(value);
    }

    /// <summary>
    /// A value of NeedGroup.
    /// </summary>
    [JsonPropertyName("needGroup")]
    public Common NeedGroup
    {
      get => needGroup ??= new();
      set => needGroup = value;
    }

    /// <summary>
    /// A value of AlternateMethod.
    /// </summary>
    [JsonPropertyName("alternateMethod")]
    public Common AlternateMethod
    {
      get => alternateMethod ??= new();
      set => alternateMethod = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of CurrentCaseRole.
    /// </summary>
    [JsonPropertyName("currentCaseRole")]
    public CaseRole CurrentCaseRole
    {
      get => currentCaseRole ??= new();
      set => currentCaseRole = value;
    }

    /// <summary>
    /// A value of PreviousCaseRole.
    /// </summary>
    [JsonPropertyName("previousCaseRole")]
    public CaseRole PreviousCaseRole
    {
      get => previousCaseRole ??= new();
      set => previousCaseRole = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of NaturalParentCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("naturalParentCsePersonsWorkSet")]
    public CsePersonsWorkSet NaturalParentCsePersonsWorkSet
    {
      get => naturalParentCsePersonsWorkSet ??= new();
      set => naturalParentCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NaturalParentCsePersonAddress.
    /// </summary>
    [JsonPropertyName("naturalParentCsePersonAddress")]
    public CsePersonAddress NaturalParentCsePersonAddress
    {
      get => naturalParentCsePersonAddress ??= new();
      set => naturalParentCsePersonAddress = value;
    }

    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    /// <summary>
    /// A value of NullPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("nullPersonPrivateAttorney")]
    public PersonPrivateAttorney NullPersonPrivateAttorney
    {
      get => nullPersonPrivateAttorney ??= new();
      set => nullPersonPrivateAttorney = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of CurrentNumberCommon.
    /// </summary>
    [JsonPropertyName("currentNumberCommon")]
    public Common CurrentNumberCommon
    {
      get => currentNumberCommon ??= new();
      set => currentNumberCommon = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public AbendData Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of NullSpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("nullSpPrintWorkSet")]
    public SpPrintWorkSet NullSpPrintWorkSet
    {
      get => nullSpPrintWorkSet ??= new();
      set => nullSpPrintWorkSet = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Field Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of CurrentField.
    /// </summary>
    [JsonPropertyName("currentField")]
    public Field CurrentField
    {
      get => currentField ??= new();
      set => currentField = value;
    }

    /// <summary>
    /// Gets a value of Address.
    /// </summary>
    [JsonIgnore]
    public Array<AddressGroup> Address => address ??= new(
      AddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Address for json serialization.
    /// </summary>
    [JsonPropertyName("address")]
    [Computed]
    public IList<AddressGroup> Address_Json
    {
      get => address;
      set => Address.Assign(value);
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// A value of ProcessGroup.
    /// </summary>
    [JsonPropertyName("processGroup")]
    public Common ProcessGroup
    {
      get => processGroup ??= new();
      set => processGroup = value;
    }

    /// <summary>
    /// A value of PreviousField.
    /// </summary>
    [JsonPropertyName("previousField")]
    public Field PreviousField
    {
      get => previousField ??= new();
      set => previousField = value;
    }

    /// <summary>
    /// A value of ChildAge.
    /// </summary>
    [JsonPropertyName("childAge")]
    public Common ChildAge
    {
      get => childAge ??= new();
      set => childAge = value;
    }

    /// <summary>
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of CurrentNumberField.
    /// </summary>
    [JsonPropertyName("currentNumberField")]
    public Field CurrentNumberField
    {
      get => currentNumberField ??= new();
      set => currentNumberField = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
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
    /// A value of GetPersonNameFlag.
    /// </summary>
    [JsonPropertyName("getPersonNameFlag")]
    public Common GetPersonNameFlag
    {
      get => getPersonNameFlag ??= new();
      set => getPersonNameFlag = value;
    }

    private Array<NaturalParentGroup> naturalParent;
    private CaseRole naturalParentCaseRole;
    private Common populatePlaceholder;
    private Common maskSsn;
    private Contact contact;
    private Field determineChFv;
    private Employer employer;
    private IncomeSource incomeSource;
    private CsePerson prFv;
    private CsePerson chFv;
    private CsePerson arFv;
    private CsePerson apFv;
    private Array<ChAlternateMethodsGroup> chAlternateMethods;
    private Array<ApAlternateMethodsGroup> apAlternateMethods;
    private Array<ApGroup> ap;
    private Common skipCh;
    private Common skipAp;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private CsePersonsWorkSet zdelLocalNaturalFather;
    private Array<ChGroup> ch;
    private Common needGroup;
    private Common alternateMethod;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonAddress csePersonAddress;
    private CaseRole currentCaseRole;
    private CaseRole previousCaseRole;
    private CodeValue codeValue;
    private Code code;
    private CsePersonsWorkSet naturalParentCsePersonsWorkSet;
    private CsePersonAddress naturalParentCsePersonAddress;
    private BatchConvertNumToText batchConvertNumToText;
    private PersonPrivateAttorney nullPersonPrivateAttorney;
    private PersonPrivateAttorney personPrivateAttorney;
    private Common currentNumberCommon;
    private AbendData read;
    private SpPrintWorkSet nullSpPrintWorkSet;
    private Common position;
    private Field temp;
    private Field currentField;
    private Array<AddressGroup> address;
    private DateWorkArea nullDateWorkArea;
    private FieldValue fieldValue;
    private Common processGroup;
    private Field previousField;
    private Common childAge;
    private SpPrintWorkSet spPrintWorkSet;
    private DateWorkArea dateWorkArea;
    private Field currentNumberField;
    private WorkArea workArea;
    private SpDocLiteral spDocLiteral;
    private ObligationType obligationType;
    private Common getPersonNameFlag;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LocateRequest.
    /// </summary>
    [JsonPropertyName("locateRequest")]
    public LocateRequest LocateRequest
    {
      get => locateRequest ??= new();
      set => locateRequest = value;
    }

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
    /// A value of ContactAddress.
    /// </summary>
    [JsonPropertyName("contactAddress")]
    public ContactAddress ContactAddress
    {
      get => contactAddress ??= new();
      set => contactAddress = value;
    }

    /// <summary>
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    /// <summary>
    /// A value of Hq.
    /// </summary>
    [JsonPropertyName("hq")]
    public Employer Hq
    {
      get => hq ??= new();
      set => hq = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
    }

    /// <summary>
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public RegisteredAgent RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    /// <summary>
    /// A value of EmployerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("employerRegisteredAgent")]
    public EmployerRegisteredAgent EmployerRegisteredAgent
    {
      get => employerRegisteredAgent ??= new();
      set => employerRegisteredAgent = value;
    }

    /// <summary>
    /// A value of OtherIncomeSource.
    /// </summary>
    [JsonPropertyName("otherIncomeSource")]
    public IncomeSource OtherIncomeSource
    {
      get => otherIncomeSource ??= new();
      set => otherIncomeSource = value;
    }

    /// <summary>
    /// A value of ZdelNaturalFather.
    /// </summary>
    [JsonPropertyName("zdelNaturalFather")]
    public CsePerson ZdelNaturalFather
    {
      get => zdelNaturalFather ??= new();
      set => zdelNaturalFather = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    /// <summary>
    /// A value of NonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("nonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress NonEmployIncomeSourceAddress
    {
      get => nonEmployIncomeSourceAddress ??= new();
      set => nonEmployIncomeSourceAddress = value;
    }

    /// <summary>
    /// A value of AdminAppealAppellantAddress.
    /// </summary>
    [JsonPropertyName("adminAppealAppellantAddress")]
    public AdminAppealAppellantAddress AdminAppealAppellantAddress
    {
      get => adminAppealAppellantAddress ??= new();
      set => adminAppealAppellantAddress = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of NaturalParentCsePerson.
    /// </summary>
    [JsonPropertyName("naturalParentCsePerson")]
    public CsePerson NaturalParentCsePerson
    {
      get => naturalParentCsePerson ??= new();
      set => naturalParentCsePerson = value;
    }

    /// <summary>
    /// A value of NaturalParentCaseRole.
    /// </summary>
    [JsonPropertyName("naturalParentCaseRole")]
    public CaseRole NaturalParentCaseRole
    {
      get => naturalParentCaseRole ??= new();
      set => naturalParentCaseRole = value;
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
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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

    private LocateRequest locateRequest;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private ContactAddress contactAddress;
    private Contact contact;
    private Employer hq;
    private EmployerRelation employerRelation;
    private Fips fips;
    private CsePersonAddress csePersonAddress;
    private RegisteredAgentAddress registeredAgentAddress;
    private RegisteredAgent registeredAgent;
    private EmployerRegisteredAgent employerRegisteredAgent;
    private IncomeSource otherIncomeSource;
    private CsePerson zdelNaturalFather;
    private InformationRequest informationRequest;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
    private AdminAppealAppellantAddress adminAppealAppellantAddress;
    private AdministrativeAppeal administrativeAppeal;
    private Field field;
    private DocumentField documentField;
    private Document document;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private LegalAction legalAction;
    private CsePerson naturalParentCsePerson;
    private CaseRole naturalParentCaseRole;
    private Case1 case1;
    private PersonPrivateAttorney personPrivateAttorney;
    private PrivateAttorneyAddress privateAttorneyAddress;
    private IncomeSource incomeSource;
    private LegalActionIncomeSource legalActionIncomeSource;
    private EmployerAddress employerAddress;
    private Employer employer;
    private ObligationType obligationType;
    private Tribunal tribunal;
  }
#endregion
}
