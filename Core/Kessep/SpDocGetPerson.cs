// Program: SP_DOC_GET_PERSON, ID: 372434851, model: 746.
// Short name: SWE02368
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DOC_GET_PERSON.
/// </summary>
[Serializable]
public partial class SpDocGetPerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_GET_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocGetPerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocGetPerson.
  /// </summary>
  public SpDocGetPerson(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------------------------
    // Date        Developer   Request #	Description
    // ---------------------------------------------------------------------------------------------------------------------
    // 03/19/1999  M Ramirez			Initial Development
    // 09/30/1999  M Ramirez	75829		Modified alternate methods for finding LEA 
    // children
    // 09/30/1999  M Ramirez	J Bickel	Removed effective date qualifier for LOPS 
    // methods
    // 09/30/1999  M Ramirez	J Bickel	Removed IWO LOPS method, replaced with WA 
    // and WC
    // 09/30/1999  M Ramirez	J Bickel	Removed checks for no LDET and for 
    // multiple LDETs
    // 11/03/1999  M Ramirez	78737		Find children for Non-Financial LDETs 
    // differently than
    // 					Financial ones.
    // 01/20/2000  PMcElderry	84668 		Allow USER to select Service Provider for 
    // NOA's.
    // 	    M Ramirez
    // 03/01/2000  M Ramirez	88238		Need to pick up enddated children for ISC
    // 03/16/2000  M Ramirez	WR 000162	Added attributes to export cse_person
    // 06/15/2000  M Ramirez	97135		Allow Org Name to be returned for Business 
    // Object 'OBL'
    // 07/03/2000  M Ramirez	WR# 173 Seg B	Added 'alternate methods' logic for 
    // Business Object 'CAS', including FV
    // 10/03/2000  M Ramirez	102858		Changed AP LA2 to account for Case Number 
    // if it is given.
    // 01/08/2001  M Ramirez	110446		In LEA and NOA, for alternate methods of 
    // finding groups, added checks for
    // 					the person already in the group.  This allows multiple alternate 
    // methods to work
    // 					in conjunction.  Also, in the event of an error from ADABAS, will 
    // continue to process
    // 					the group before escaping so that if one person has an ADABAS error 
    // it won't mess
    // 					up the entire document.
    // 01/08/2001  M Ramirez	none		Marked export_error_ind zdel
    // 03/21/2001  M Ramirez	WR 291		Added LOC
    // 			Seg B
    // 09/20/2001  M Ramirez	124861		Added formatted name to the exports
    // 09/21/2001  M Ramirez	119209		Changed AP LA5 to look for 'I' 	class legal
    // actions, instead of just IWOs
    // 02/05/2002  M Ramirez	PR120947	Changed AP LA1 to account for Case Number 
    // if it is given.
    // 03/20/2002  K Cole   	PR127439	Populate company name if there is no 
    // contact name for "other policy holder" on health insurance.
    // 08/11/2004  M Quinn	PR 213100	Use the same case number for the AR if a 
    // case number has already been identified for the AP.
    // 03/13/2007  GVandy	PR284898	Performance change when reading AP LA1 for 
    // LEA documents.
    // 11/09/2009  JHuss	CQ 12826	Added created timestamp sort criteria for LEA 
    // documents.
    // 01/24/2011  GVandy	CQ24707		An additional performance change required due
    // to rebinds following
    // 					the partitioning of the CASE_ROLE table.
    // ---------------------------------------------------------------------------------------------------------------------
    if (Lt(local.Null1.Date, import.Current.Date))
    {
      local.Current.Date = import.Current.Date;
    }
    else
    {
      local.Current.Date = Now().Date;
    }

    export.SpDocKey.Assign(import.SpDocKey);

    if (AsChar(import.ReturnGroup.Flag) == 'Y')
    {
      // mjr
      // -----------------------------------------------------------------
      // Calling program requests a group
      // This is a group of people that fulfill a specific role for a
      // specific business object.  Groups are normally limited to children
      // and Absent Parents.
      // --------------------------------------------------------------------
      export.Export1.Index = -1;

      switch(TrimEnd(import.Document.BusinessObject))
      {
        case "APT":
          if (!Lt(local.Null1.Timestamp, export.SpDocKey.KeyAppointment))
          {
            return;
          }

          if (!ReadCase4())
          {
            return;
          }

          foreach(var item in ReadCsePerson25())
          {
            if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
            {
              break;
            }

            ++export.Export1.Index;
            export.Export1.CheckSize();

            export.Export1.Update.GcsePersonsWorkSet.Number =
              entities.CsePerson.Number;

            if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
            {
              break;
            }
          }

          break;
        case "CAS":
          if (import.AlternateMethods.Count > 0)
          {
            for(import.AlternateMethods.Index = 0; import
              .AlternateMethods.Index < import.AlternateMethods.Count; ++
              import.AlternateMethods.Index)
            {
              if (!import.AlternateMethods.CheckSize())
              {
                break;
              }

              local.ObligationType.Code = import.AlternateMethods.Item.G.Code;

              if (Equal(local.ObligationType.Code, "IGNEND"))
              {
                foreach(var item in ReadCsePerson26())
                {
                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    goto AfterCycle1;
                  }

                  ++export.Export1.Index;
                  export.Export1.CheckSize();

                  export.Export1.Update.GcsePersonsWorkSet.Number =
                    entities.CsePerson.Number;

                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    goto AfterCycle1;
                  }
                }
              }
              else if (Equal(local.ObligationType.Code, "FV"))
              {
                if (ReadCsePerson9())
                {
                  local.Temp.Date = entities.CsePerson.FvLetterSentDate;

                  // mjr
                  // -------------------------------------------------------
                  // 07/03/2000
                  // Select only the children on the case that have the same 
                  // Send Date as the AR.
                  // --------------------------------------------------------------------
                  foreach(var item in ReadCsePerson24())
                  {
                    // mjr
                    // -------------------------------------------------------
                    // 07/03/2000
                    // Since we are ignoring Start and End dates, we need to
                    // make sure we are not showing duplicate children.
                    // --------------------------------------------------------------------
                    for(export.Export1.Index = 0; export.Export1.Index < export
                      .Export1.Count; ++export.Export1.Index)
                    {
                      if (!export.Export1.CheckSize())
                      {
                        break;
                      }

                      if (Equal(entities.CsePerson.Number,
                        export.Export1.Item.GcsePersonsWorkSet.Number))
                      {
                        goto ReadEach1;
                      }
                    }

                    export.Export1.CheckIndex();

                    export.Export1.Index = export.Export1.Count - 1;
                    export.Export1.CheckSize();

                    if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                    {
                      goto AfterCycle1;
                    }

                    export.Export1.Index = export.Export1.Count;
                    export.Export1.CheckSize();

                    export.Export1.Update.GcsePersonsWorkSet.Number =
                      entities.CsePerson.Number;

                    if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                    {
                      goto AfterCycle1;
                    }

ReadEach1:
                    ;
                  }
                }
              }
              else
              {
              }
            }

AfterCycle1:

            import.AlternateMethods.CheckIndex();
          }
          else
          {
            foreach(var item in ReadCsePerson23())
            {
              if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
              {
                break;
              }

              ++export.Export1.Index;
              export.Export1.CheckSize();

              export.Export1.Update.GcsePersonsWorkSet.Number =
                entities.CsePerson.Number;

              if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
              {
                break;
              }
            }
          }

          break;
        case "ISC":
          // mjr---> Interstate case
          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (export.SpDocKey.KeyInterstateRequest <= 0)
            {
              return;
            }

            if (ReadCase5())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }
            else
            {
              return;
            }
          }

          // mjr
          // ----------------------------------------------
          // 03/01/2000
          // PR# 88238 - Need to pick up enddated children
          // Relaxed enddate check
          // -----------------------------------------------------------
          foreach(var item in ReadCsePerson26())
          {
            if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
            {
              break;
            }

            ++export.Export1.Index;
            export.Export1.CheckSize();

            export.Export1.Update.GcsePersonsWorkSet.Number =
              entities.CsePerson.Number;

            if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
            {
              break;
            }
          }

          break;
        case "LAD":
          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AP":
              local.LegalActionPerson.AccountType = "R";

              break;
            case "CH":
              local.LegalActionPerson.AccountType = "S";

              break;
            default:
              break;
          }

          foreach(var item in ReadCsePerson22())
          {
            if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
            {
              break;
            }

            ++export.Export1.Index;
            export.Export1.CheckSize();

            export.Export1.Update.GcsePersonsWorkSet.Number =
              entities.CsePerson.Number;

            if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
            {
              break;
            }
          }

          break;
        case "LEA":
          // mjr
          // --------------------------------------------------------
          // Alternate method for a group of people using business
          // object "LEA" means to use LOPS.
          // Default method is to use LROL.
          // -----------------------------------------------------------
          if (import.AlternateMethods.Count > 0)
          {
            for(import.AlternateMethods.Index = 0; import
              .AlternateMethods.Index < import.AlternateMethods.Count; ++
              import.AlternateMethods.Index)
            {
              if (!import.AlternateMethods.CheckSize())
              {
                break;
              }

              local.ObligationType.Code = import.AlternateMethods.Item.G.Code;

              if (Equal(local.ObligationType.Code, "IGNEND"))
              {
                // mjr
                // --------------------------------------------------------------
                // Find all case roles (current and ended)
                // Same as default method, without case role dates qualifiers
                // -----------------------------------------------------------------
                foreach(var item in ReadCsePerson20())
                {
                  for(export.Export1.Index = 0; export.Export1.Index < export
                    .Export1.Count; ++export.Export1.Index)
                  {
                    if (!export.Export1.CheckSize())
                    {
                      break;
                    }

                    if (Equal(entities.CsePerson.Number,
                      export.Export1.Item.GcsePersonsWorkSet.Number))
                    {
                      goto ReadEach2;
                    }
                  }

                  export.Export1.CheckIndex();

                  export.Export1.Index = export.Export1.Count - 1;
                  export.Export1.CheckSize();

                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    goto AfterCycle2;
                  }

                  export.Export1.Index = export.Export1.Count;
                  export.Export1.CheckSize();

                  export.Export1.Update.GcsePersonsWorkSet.Number =
                    entities.CsePerson.Number;

                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    goto AfterCycle2;
                  }

ReadEach2:
                  ;
                }
              }
              else
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    local.LegalActionPerson.AccountType = "R";

                    break;
                  case "CH":
                    local.LegalActionPerson.AccountType = "S";

                    break;
                  default:
                    break;
                }

                if (ReadObligationType())
                {
                  // mjr
                  // -----------------------------------------------------------
                  // 11/02/1999
                  // Financial LDET type
                  // ------------------------------------------------------------------------
                  foreach(var item in ReadLegalActionDetail2())
                  {
                    local.LegalActionDetail.Number =
                      entities.LegalActionDetail.Number;

                    foreach(var item1 in ReadCsePerson21())
                    {
                      for(export.Export1.Index = 0; export.Export1.Index < export
                        .Export1.Count; ++export.Export1.Index)
                      {
                        if (!export.Export1.CheckSize())
                        {
                          break;
                        }

                        if (Equal(entities.CsePerson.Number,
                          export.Export1.Item.GcsePersonsWorkSet.Number))
                        {
                          goto ReadEach3;
                        }
                      }

                      export.Export1.CheckIndex();

                      export.Export1.Index = export.Export1.Count - 1;
                      export.Export1.CheckSize();

                      if (export.Export1.Index + 1 >= Export
                        .ExportGroup.Capacity)
                      {
                        goto ReadEach4;
                      }

                      export.Export1.Index = export.Export1.Count;
                      export.Export1.CheckSize();

                      export.Export1.Update.GcsePersonsWorkSet.Number =
                        entities.CsePerson.Number;

                      if (export.Export1.Index + 1 >= Export
                        .ExportGroup.Capacity)
                      {
                        goto ReadEach4;
                      }

ReadEach3:
                      ;
                    }
                  }

ReadEach4:
                  ;
                }
                else
                {
                  // mjr
                  // -----------------------------------------------------------
                  // 11/02/1999
                  // Non-Financial LDET type
                  // ------------------------------------------------------------------------
                  foreach(var item in ReadLegalActionDetail1())
                  {
                    local.LegalActionDetail.Number =
                      entities.LegalActionDetail.Number;

                    foreach(var item1 in ReadCsePerson21())
                    {
                      for(export.Export1.Index = 0; export.Export1.Index < export
                        .Export1.Count; ++export.Export1.Index)
                      {
                        if (!export.Export1.CheckSize())
                        {
                          break;
                        }

                        if (Equal(entities.CsePerson.Number,
                          export.Export1.Item.GcsePersonsWorkSet.Number))
                        {
                          goto ReadEach5;
                        }
                      }

                      export.Export1.CheckIndex();

                      export.Export1.Index = export.Export1.Count - 1;
                      export.Export1.CheckSize();

                      if (export.Export1.Index + 1 >= Export
                        .ExportGroup.Capacity)
                      {
                        goto ReadEach6;
                      }

                      export.Export1.Index = export.Export1.Count;
                      export.Export1.CheckSize();

                      export.Export1.Update.GcsePersonsWorkSet.Number =
                        entities.CsePerson.Number;

                      if (export.Export1.Index + 1 >= Export
                        .ExportGroup.Capacity)
                      {
                        goto ReadEach6;
                      }

ReadEach5:
                      ;
                    }
                  }

ReadEach6:
                  ;
                }
              }
            }

AfterCycle2:

            import.AlternateMethods.CheckIndex();
          }
          else
          {
            foreach(var item in ReadCsePerson19())
            {
              if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
              {
                break;
              }

              ++export.Export1.Index;
              export.Export1.CheckSize();

              export.Export1.Update.GcsePersonsWorkSet.Number =
                entities.CsePerson.Number;

              if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
              {
                break;
              }
            }
          }

          break;
        case "NOA":
          // mjr
          // --------------------------------------------------------
          // Alternate method for a group of people using business
          // object "LEA" means to use LOPS.
          // Default method is to use LROL.
          // -----------------------------------------------------------
          if (import.AlternateMethods.Count > 0)
          {
            for(import.AlternateMethods.Index = 0; import
              .AlternateMethods.Index < import.AlternateMethods.Count; ++
              import.AlternateMethods.Index)
            {
              if (!import.AlternateMethods.CheckSize())
              {
                break;
              }

              local.ObligationType.Code = import.AlternateMethods.Item.G.Code;

              if (Equal(local.ObligationType.Code, "IGNEND"))
              {
                // mjr
                // --------------------------------------------------------------
                // Find all case roles (current and ended)
                // Same as default method, without case role dates qualifiers
                // -----------------------------------------------------------------
                foreach(var item in ReadCsePerson20())
                {
                  for(export.Export1.Index = 0; export.Export1.Index < export
                    .Export1.Count; ++export.Export1.Index)
                  {
                    if (!export.Export1.CheckSize())
                    {
                      break;
                    }

                    if (Equal(entities.CsePerson.Number,
                      export.Export1.Item.GcsePersonsWorkSet.Number))
                    {
                      goto ReadEach7;
                    }
                  }

                  export.Export1.CheckIndex();

                  export.Export1.Index = export.Export1.Count - 1;
                  export.Export1.CheckSize();

                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    goto AfterCycle3;
                  }

                  export.Export1.Index = export.Export1.Count;
                  export.Export1.CheckSize();

                  export.Export1.Update.GcsePersonsWorkSet.Number =
                    entities.CsePerson.Number;

                  if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
                  {
                    goto AfterCycle3;
                  }

ReadEach7:
                  ;
                }
              }
              else
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    local.LegalActionPerson.AccountType = "R";

                    break;
                  case "CH":
                    local.LegalActionPerson.AccountType = "S";

                    break;
                  default:
                    break;
                }

                if (ReadObligationType())
                {
                  // mjr
                  // -----------------------------------------------------------
                  // 11/02/1999
                  // Financial LDET type
                  // ------------------------------------------------------------------------
                  foreach(var item in ReadLegalActionDetail2())
                  {
                    local.LegalActionDetail.Number =
                      entities.LegalActionDetail.Number;

                    foreach(var item1 in ReadCsePerson21())
                    {
                      for(export.Export1.Index = 0; export.Export1.Index < export
                        .Export1.Count; ++export.Export1.Index)
                      {
                        if (!export.Export1.CheckSize())
                        {
                          break;
                        }

                        if (Equal(entities.CsePerson.Number,
                          export.Export1.Item.GcsePersonsWorkSet.Number))
                        {
                          goto ReadEach8;
                        }
                      }

                      export.Export1.CheckIndex();

                      export.Export1.Index = export.Export1.Count - 1;
                      export.Export1.CheckSize();

                      if (export.Export1.Index + 1 >= Export
                        .ExportGroup.Capacity)
                      {
                        goto ReadEach9;
                      }

                      export.Export1.Index = export.Export1.Count;
                      export.Export1.CheckSize();

                      export.Export1.Update.GcsePersonsWorkSet.Number =
                        entities.CsePerson.Number;

                      if (export.Export1.Index + 1 >= Export
                        .ExportGroup.Capacity)
                      {
                        goto ReadEach9;
                      }

ReadEach8:
                      ;
                    }
                  }

ReadEach9:
                  ;
                }
                else
                {
                  // mjr
                  // -----------------------------------------------------------
                  // 11/02/1999
                  // Non-Financial LDET type
                  // ------------------------------------------------------------------------
                  foreach(var item in ReadLegalActionDetail1())
                  {
                    local.LegalActionDetail.Number =
                      entities.LegalActionDetail.Number;

                    foreach(var item1 in ReadCsePerson21())
                    {
                      for(export.Export1.Index = 0; export.Export1.Index < export
                        .Export1.Count; ++export.Export1.Index)
                      {
                        if (!export.Export1.CheckSize())
                        {
                          break;
                        }

                        if (Equal(entities.CsePerson.Number,
                          export.Export1.Item.GcsePersonsWorkSet.Number))
                        {
                          goto ReadEach10;
                        }
                      }

                      export.Export1.CheckIndex();

                      export.Export1.Index = export.Export1.Count - 1;
                      export.Export1.CheckSize();

                      if (export.Export1.Index + 1 >= Export
                        .ExportGroup.Capacity)
                      {
                        goto ReadEach11;
                      }

                      export.Export1.Index = export.Export1.Count;
                      export.Export1.CheckSize();

                      export.Export1.Update.GcsePersonsWorkSet.Number =
                        entities.CsePerson.Number;

                      if (export.Export1.Index + 1 >= Export
                        .ExportGroup.Capacity)
                      {
                        goto ReadEach11;
                      }

ReadEach10:
                      ;
                    }
                  }

ReadEach11:
                  ;
                }
              }
            }

AfterCycle3:

            import.AlternateMethods.CheckIndex();
          }
          else
          {
            foreach(var item in ReadCsePerson19())
            {
              if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
              {
                break;
              }

              ++export.Export1.Index;
              export.Export1.CheckSize();

              export.Export1.Update.GcsePersonsWorkSet.Number =
                entities.CsePerson.Number;

              if (export.Export1.Index + 1 >= Export.ExportGroup.Capacity)
              {
                break;
              }
            }
          }

          break;
        default:
          break;
      }

      // mjr
      // -------------------------------------------------------------
      // Now populate group_export
      // ----------------------------------------------------------------
      if (export.Export1.Count <= 0)
      {
        return;
      }

      export.Export1.Index = 0;

      for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
        export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (IsEmpty(export.Export1.Item.GcsePersonsWorkSet.Number))
        {
          return;
        }

        if (AsChar(import.Batch.Flag) == 'Y')
        {
          UseSiReadCsePersonBatch2();

          if (!IsEmpty(local.Read.Type1) && Equal
            (local.Read.AdabasResponseCd, "0148"))
          {
            export.ErrorDocumentField.ScreenPrompt = "Resource Error";
            export.ErrorFieldValue.Value = "ADABAS Unavailable";
          }
        }
        else
        {
          UseSiReadCsePerson2();

          if (!IsEmpty(local.Read.Type1))
          {
            export.ErrorDocumentField.ScreenPrompt = "ABEND";
            export.ErrorFieldValue.Value = "SI_READ_CSE_PERSON";
          }
        }

        if (!IsEmpty(local.Read.Type1) || !IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      export.Export1.CheckIndex();
    }
    else
    {
      switch(TrimEnd(import.Document.BusinessObject))
      {
        case "AAC":
          // mjr---> Must be passed in
          // mjr---> Only applicable for PR
          break;
        case "ADA":
          if (export.SpDocKey.KeyAdminAppeal <= 0)
          {
            return;
          }

          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AR":
              if (IsEmpty(export.SpDocKey.KeyCase))
              {
                local.Count.Count = 0;

                foreach(var item in ReadCase6())
                {
                  ++local.Count.Count;

                  if (local.Count.Count > 1)
                  {
                    return;
                  }

                  export.SpDocKey.KeyCase = entities.Case1.Number;
                }

                if (local.Count.Count < 1)
                {
                  return;
                }
              }

              if (ReadCsePerson8())
              {
                export.SpDocKey.KeyAr = entities.CsePerson.Number;
              }

              break;
            case "PR":
              if (!ReadAdministrativeAppeal())
              {
                return;
              }

              if (!IsEmpty(entities.AdministrativeAppeal.AppellantLastName))
              {
                export.SpDocKey.KeyPerson = "AAPPEAL";
              }
              else if (ReadCsePerson16())
              {
                export.SpDocKey.KeyPerson = entities.CsePerson.Number;
              }

              break;
            default:
              break;
          }

          break;
        case "APT":
          if (!Lt(local.Null1.Timestamp, export.SpDocKey.KeyAppointment))
          {
            return;
          }

          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AP":
              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // First check sp_doc_key to determine if the user has
              // already selected the <AP,AR,CH or PR> they are using.
              // -----------------------------------------------------------
              if (!IsEmpty(export.SpDocKey.KeyAp))
              {
                break;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No AP was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              if (!ReadCase4())
              {
                return;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // Check for multiples of <AP,AR or CH> role codes for the case.
              // -----------------------------------------------------------
              local.Count.Count = 0;

              foreach(var item in ReadCsePerson25())
              {
                ++local.Count.Count;
              }

              if (local.Count.Count < 1)
              {
                // mjr
                // ----------------------------------------------
                // 12/03/1998
                // No <AP,AR or CH> was found.  Skip all fields for this
                // role code
                // -----------------------------------------------------------
                return;
              }

              if (local.Count.Count > 1)
              {
                // mjr
                // ----------------------------------------------
                // 12/03/1998
                // Because there are multiples of <AP,AR or CH>, the user
                // needs to tell us which <AP,AR,CH or PR> they are using.
                // Skip all fields for this role code.
                // -----------------------------------------------------------
                return;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // The case only has one <AP,AR,CH or PR>, so we use that one.
              // -----------------------------------------------------------
              export.SpDocKey.KeyAp = entities.CsePerson.Number;

              break;
            case "AR":
              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // First check sp_doc_key to determine if the user has
              // already selected the <AP,AR,CH or PR> they are using.
              // -----------------------------------------------------------
              if (!IsEmpty(export.SpDocKey.KeyAr))
              {
                break;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No AP was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              if (!ReadCase4())
              {
                return;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // Check for multiples of <AP,AR or CH> role codes for the case.
              // -----------------------------------------------------------
              local.Count.Count = 0;

              foreach(var item in ReadCsePerson25())
              {
                ++local.Count.Count;
              }

              if (local.Count.Count < 1)
              {
                // mjr
                // ----------------------------------------------
                // 12/03/1998
                // No <AP,AR or CH> was found.  Skip all fields for this
                // role code
                // -----------------------------------------------------------
                return;
              }

              if (local.Count.Count > 1)
              {
                // mjr
                // ----------------------------------------------
                // 12/03/1998
                // Because there are multiples of <AP,AR or CH>, the user
                // needs to tell us which <AP,AR,CH or PR> they are using.
                // Skip all fields for this role code.
                // -----------------------------------------------------------
                return;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // The case only has one <AP,AR,CH or PR>, so we use that one.
              // -----------------------------------------------------------
              export.SpDocKey.KeyAr = entities.CsePerson.Number;

              break;
            case "PR":
              if (ReadCsePerson11())
              {
                export.SpDocKey.KeyPerson = entities.CsePerson.Number;
              }

              break;
            default:
              break;
          }

          break;
        case "BKR":
          // mjr---> Must be passed in
          // mjr---> Only applicable for PR
          break;
        case "CAS":
          // mjr
          // ----------------------------------------------
          // 12/03/1998
          // First check sp_doc_key to determine if the user has
          // already selected the <AP,AR,CH or PR> they are using.
          // -----------------------------------------------------------
          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AP":
              if (!IsEmpty(export.SpDocKey.KeyAp))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No AP was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              break;
            case "AR":
              if (!IsEmpty(export.SpDocKey.KeyAr))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No AR was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              break;
            case "CH":
              if (!IsEmpty(export.SpDocKey.KeyChild))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No CH was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              break;
            case "PR":
              if (!IsEmpty(export.SpDocKey.KeyPerson))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No PR was designated by the user.  Skip all fields for this
              // role code
              // -----------------------------------------------------------
              return;
            default:
              return;
          }

          // mjr
          // ----------------------------------------------
          // 12/03/1998
          // Check for multiples of <AP,AR or CH> role codes for the case.
          // -----------------------------------------------------------
          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (!IsEmpty(export.SpDocKey.KeyChild))
            {
              if (ReadCase3())
              {
                export.SpDocKey.KeyCase = entities.Case1.Number;
              }
            }
          }

          local.Count.Count = 0;

          foreach(var item in ReadCsePerson23())
          {
            ++local.Count.Count;
          }

          if (local.Count.Count < 1)
          {
            // mjr
            // ----------------------------------------------
            // 12/03/1998
            // No <AP,AR or CH> was found.  Skip all fields for this
            // role code
            // -----------------------------------------------------------
            return;
          }

          if (local.Count.Count > 1)
          {
            // mjr
            // ----------------------------------------------
            // 12/03/1998
            // Because there are multiples of <AP,AR or CH>, the user
            // needs to tell us which <AP,AR,CH or PR> they are using.
            // Skip all fields for this role code.
            // -----------------------------------------------------------
            return;
          }

          // mjr
          // ----------------------------------------------
          // 12/03/1998
          // The case only has one <AP,AR,CH or PR>, so we use that one.
          // -----------------------------------------------------------
          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AP":
              export.SpDocKey.KeyAp = entities.CsePerson.Number;

              break;
            case "AR":
              export.SpDocKey.KeyAr = entities.CsePerson.Number;

              break;
            case "CH":
              export.SpDocKey.KeyChild = entities.CsePerson.Number;

              break;
            default:
              break;
          }

          break;
        case "CRD":
          if (Equal(import.CaseRole.Type1, "PR"))
          {
            if (!IsEmpty(export.SpDocKey.KeyPerson))
            {
              break;
            }

            if (export.SpDocKey.KeyCashRcptDetail > 0 && export
              .SpDocKey.KeyCashRcptEvent > 0 && export
              .SpDocKey.KeyCashRcptSource > 0 && export
              .SpDocKey.KeyCashRcptType > 0)
            {
              if (ReadCashReceiptDetail())
              {
                export.SpDocKey.KeyPerson =
                  entities.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
              }
            }
          }
          else
          {
          }

          break;
        case "GNT":
          if (export.SpDocKey.KeyGeneticTest <= 0)
          {
            return;
          }

          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AP":
              if (ReadCsePerson13())
              {
                export.SpDocKey.KeyAp = entities.CsePerson.Number;
              }

              break;
            case "AR":
              if (ReadCsePerson7())
              {
                export.SpDocKey.KeyAr = entities.CsePerson.Number;
              }

              break;
            case "CH":
              if (ReadCsePerson14())
              {
                export.SpDocKey.KeyChild = entities.CsePerson.Number;
              }

              break;
            default:
              break;
          }

          break;
        case "IRQ":
          if (export.SpDocKey.KeyInfoRequest == 0)
          {
            return;
          }

          if (Equal(import.CaseRole.Type1, "PR"))
          {
            if (ReadInformationRequest())
            {
              // mjr
              // ------------------------------------------------------------
              // Information request is used later, but PR number needs to be 
              // set
              // -----------------------------------------------------------------
              export.SpDocKey.KeyPerson = "INFO REQ";
            }
          }
          else
          {
          }

          break;
        case "ISC":
          if (export.SpDocKey.KeyInterstateRequest <= 0)
          {
            return;
          }

          // mjr
          // ----------------------------------------------
          // 12/03/1998
          // First check sp_doc_key to determine if the user has
          // already selected the <AP,AR,CH or PR> they are using.
          // -----------------------------------------------------------
          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AP":
              if (!IsEmpty(export.SpDocKey.KeyAp))
              {
                goto Test;
              }

              if (ReadCsePerson12())
              {
                export.SpDocKey.KeyAp = entities.CsePerson.Number;

                goto Test;
              }
              else
              {
                return;
              }

              break;
            case "AR":
              if (!IsEmpty(export.SpDocKey.KeyAr))
              {
                if (ReadCsePerson17())
                {
                  if (AsChar(entities.CsePerson.Type1) == 'O')
                  {
                    export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
                    export.CsePerson.Number = entities.CsePerson.Number;
                    export.CsePersonsWorkSet.FirstName =
                      entities.CsePerson.OrganizationName ?? Spaces(12);

                    return;
                  }

                  goto Test;
                }
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No AR was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              break;
            case "CH":
              if (!IsEmpty(export.SpDocKey.KeyChild))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No CH was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              break;
            case "PR":
              if (!IsEmpty(export.SpDocKey.KeyPerson))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 12/03/1998
              // No PR was designated by the user.  Skip all fields for this
              // role code
              // -----------------------------------------------------------
              return;
            default:
              return;
          }

          // mjr
          // ----------------------------------------------
          // 12/03/1998
          // Check for multiples of <AR or CH> role codes for the case.
          // -----------------------------------------------------------
          if (IsEmpty(export.SpDocKey.KeyCase))
          {
            if (ReadCase5())
            {
              export.SpDocKey.KeyCase = entities.Case1.Number;
            }
            else
            {
              return;
            }
          }

          local.Count.Count = 0;

          foreach(var item in ReadCsePerson23())
          {
            ++local.Count.Count;
          }

          if (local.Count.Count < 1)
          {
            // mjr
            // ----------------------------------------------
            // 12/03/1998
            // No <AR or CH> was found.  Skip all fields for this
            // role code
            // -----------------------------------------------------------
            return;
          }

          if (local.Count.Count > 1)
          {
            // mjr
            // ----------------------------------------------
            // 12/03/1998
            // Because there are multiples of <AR or CH>, the user
            // needs to tell us which <AR or CH> they are using.
            // Skip all fields for this role code.
            // -----------------------------------------------------------
            return;
          }

          // mjr
          // ----------------------------------------------
          // 12/03/1998
          // The case only has one <AR or CH>, so we use that one.
          // -----------------------------------------------------------
          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AR":
              export.SpDocKey.KeyAr = entities.CsePerson.Number;

              if (AsChar(entities.CsePerson.Type1) == 'O')
              {
                export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
                export.CsePerson.Number = entities.CsePerson.Number;
                export.CsePersonsWorkSet.FirstName =
                  entities.CsePerson.OrganizationName ?? Spaces(12);

                return;
              }

              break;
            case "CH":
              export.SpDocKey.KeyChild = entities.CsePerson.Number;

              break;
            default:
              break;
          }

          break;
        case "LAD":
          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "PR":
              if (IsEmpty(export.SpDocKey.KeyPerson))
              {
                return;
              }

              break;
            case "CH":
              if (IsEmpty(export.SpDocKey.KeyChild))
              {
                return;
              }

              break;
            default:
              break;
          }

          break;
        case "LEA":
          if (Equal(import.CaseRole.Type1, "PR"))
          {
            break;
          }

          // mjr
          // ---------------------------------------------------------
          // Methods for finding an AP on a Legal Action
          // AP LA1  --- default, if no other method is found use this one;
          // AP LA2  --- included on documents IWOMODO, IWOTERM and MWO;
          // AP LA3  --- included on documents EMPMWOJ, TERMMWOM and TERMMWOO;
          // AP LA4  --- included on documents GARNO and GARNAFFT;
          // AP LA5  --- included on document EMPIWOJ
          // AP LA6  --- included on document REQMWO
          // ------------------------------------------------------------
          if (AsChar(import.AlternateMethod.Flag) == '2')
          {
            // mjr
            // -----------------------------------------------
            // 10/03/2000
            // PR 102858 - Changed AP LA2 to account for Case Number
            // if it is given.
            // -----------------------------------------------------------
            // ------------------------------------------------------------
            // Read AP from most recent IWGL.
            // Legal_Action_Income_Source.Withholding_Type <> spaces ==> IWO/MWO
            // record.
            // -----------------------------------------------------------
            if (!IsEmpty(export.SpDocKey.KeyCase))
            {
              if (ReadCsePersonLegalActionIncomeSource1())
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    export.SpDocKey.KeyAp = entities.CsePerson.Number;

                    break;
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }
              }
            }
            else if (ReadCsePersonLegalActionIncomeSource2())
            {
              switch(TrimEnd(import.CaseRole.Type1))
              {
                case "AP":
                  export.SpDocKey.KeyAp = entities.CsePerson.Number;

                  break;
                case "AR":
                  export.SpDocKey.KeyAr = entities.CsePerson.Number;

                  break;
                case "CH":
                  export.SpDocKey.KeyChild = entities.CsePerson.Number;

                  break;
                default:
                  break;
              }
            }
          }
          else if (AsChar(import.AlternateMethod.Flag) == '3')
          {
            if (!ReadLegalAction3())
            {
              return;
            }

            if (!ReadTribunal())
            {
              return;
            }

            // mlb - PR00259407 - 01/13/2006 - The original read each statement 
            // is commented out
            // and replaced with a read each that also checks for the character 
            // string "MWONOTHC".
            // This is to allow the new one to be picked up whether or not the 
            // characters "MWO" are found.
            // 11/09/2009  JHuss	CQ 12826	Added created timestamp sort criteria.
            if (ReadLegalAction2())
            {
              if (ReadCsePersonLegalActionIncomeSource5())
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    export.SpDocKey.KeyAp = entities.CsePerson.Number;

                    break;
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }
              }
            }

            // end
          }
          else if (AsChar(import.AlternateMethod.Flag) == '4')
          {
            if (ReadCsePersonLegalActionIncomeSource3())
            {
              switch(TrimEnd(import.CaseRole.Type1))
              {
                case "AP":
                  export.SpDocKey.KeyAp = entities.CsePerson.Number;

                  break;
                case "AR":
                  export.SpDocKey.KeyAr = entities.CsePerson.Number;

                  break;
                case "CH":
                  export.SpDocKey.KeyChild = entities.CsePerson.Number;

                  break;
                default:
                  break;
              }
            }

            if (ReadCsePersonLegalActionPersonResource())
            {
              if (Lt(entities.LegalActionIncomeSource.EffectiveDate,
                entities.LegalActionPersonResource.EffectiveDate))
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    export.SpDocKey.KeyAp = entities.CsePerson.Number;

                    break;
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }
              }
            }
          }
          else if (AsChar(import.AlternateMethod.Flag) == '5')
          {
            // -----------------------------------------------------------------------
            // Rread AP from most recent IWGL attached to most recent IWO legal 
            // action.
            // Legal_Action_Income_Source.Withholding_Type <> spaces ==> IWO/MWO
            // record.
            // ---------------------------------------------------------------------
            if (!ReadLegalAction3())
            {
              return;
            }

            if (!ReadTribunal())
            {
              return;
            }

            foreach(var item in ReadLegalAction4())
            {
              if (!Equal(entities.FindApLegalAction.ActionTaken, "IWO") && !
                Equal(entities.FindApLegalAction.ActionTaken, "IWOMODO") && !
                Equal(entities.FindApLegalAction.ActionTaken, "IWONOTKS") && !
                Equal(entities.FindApLegalAction.ActionTaken, "IWONOTKM") && !
                Equal(entities.FindApLegalAction.ActionTaken, "ORDIWO2"))
              {
                continue;
              }

              if (ReadCsePersonLegalActionIncomeSource5())
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    export.SpDocKey.KeyAp = entities.CsePerson.Number;

                    break;
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }

                break;
              }
            }
          }
          else if (AsChar(import.AlternateMethod.Flag) == '6')
          {
            // -----------------------------------------------------------------------
            // For the following documents, read AP from the latest legal
            // action of class 'O' or 'J', that has a legal detail of type 'HIC
            // '.
            // ---------------------------------------------------------------------
            if (!ReadLegalAction3())
            {
              return;
            }

            if (!ReadTribunal())
            {
              return;
            }

            foreach(var item in ReadLegalAction5())
            {
              foreach(var item1 in ReadLegalActionDetail3())
              {
                if (ReadCsePerson1())
                {
                  switch(TrimEnd(import.CaseRole.Type1))
                  {
                    case "AP":
                      export.SpDocKey.KeyAp = entities.CsePerson.Number;

                      break;
                    case "AR":
                      export.SpDocKey.KeyAr = entities.CsePerson.Number;

                      break;
                    case "CH":
                      export.SpDocKey.KeyChild = entities.CsePerson.Number;

                      break;
                    default:
                      break;
                  }

                  goto ReadEach12;
                }
              }
            }

ReadEach12:
            ;
          }
          else
          {
            // mjr
            // -----------------------------------------------
            // 02/05/2002
            // PR 120947 - Changed AP LA1 to account for Case Number
            // if it is given.
            // -----------------------------------------------------------
            if (!IsEmpty(export.SpDocKey.KeyCase))
            {
              // --03/13/2007  GVandy  PR284898  Performance change when reading
              // AP LA1 for LEA documents.
              // --01/24/2011  GVandy  CQ24707  An additional performance change
              // required due to rebinds following
              //    the partitioning of the CASE_ROLE table.  Trying to get the 
              // path back to CKI03257 to CKI01505
              //    to CKI01315 to CKI01504.  Adding "or that case number is 
              // equal to spaces" should keep it from using
              //    CKI01505 as the first index in the path.
              if (ReadCsePerson5())
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    export.SpDocKey.KeyAp = entities.CsePerson.Number;

                    break;
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }
              }
            }
            else if (!IsEmpty(export.SpDocKey.KeyAp))
            {
              if (Equal(import.CaseRole.Type1, "AP"))
              {
                if (ReadCsePerson3())
                {
                  break;
                }

                // mjr
                // ------------------------------------------------
                // 04/11/2002
                // The AP given is not an active AP on the legal action.
                // Do not give any information about the AP
                // -------------------------------------------------------------
                return;
              }
              else if (ReadCsePerson4())
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }
              }
            }
            else if (ReadCsePerson6())
            {
              switch(TrimEnd(import.CaseRole.Type1))
              {
                case "AP":
                  export.SpDocKey.KeyAp = entities.CsePerson.Number;

                  break;
                case "AR":
                  export.SpDocKey.KeyAr = entities.CsePerson.Number;

                  break;
                case "CH":
                  export.SpDocKey.KeyChild = entities.CsePerson.Number;

                  break;
                default:
                  break;
              }
            }
          }

          break;
        case "LOC":
          // mjr---> Must be passed in
          break;
        case "NOA":
          if (Equal(import.CaseRole.Type1, "PR"))
          {
            break;
          }

          // mjr
          // ---------------------------------------------------------
          // Methods for finding an AP on a Legal Action
          // AP LA1  --- default, if no other method is found use this one;
          // AP LA2  --- included on documents IWOMODO, IWOTERM and MWO;
          // AP LA3  --- included on documents EMPMWOJ, TERMMWOM and TERMMWOO;
          // AP LA4  --- included on documents GARNO and GARNAFFT;
          // AP LA5  --- included on document EMPIWOJ
          // AP LA6  --- included on document REQMWO
          // ------------------------------------------------------------
          if (AsChar(import.AlternateMethod.Flag) == '2')
          {
            // ------------------------------------------------------------
            // Read AP from most recent IWGL.
            // Legal_Action_Income_Source.Withholding_Type <> spaces ==> IWO/MWO
            // record.
            // -----------------------------------------------------------
            if (ReadCsePersonLegalActionIncomeSource4())
            {
              switch(TrimEnd(import.CaseRole.Type1))
              {
                case "AP":
                  export.SpDocKey.KeyAp = entities.CsePerson.Number;

                  break;
                case "AR":
                  export.SpDocKey.KeyAr = entities.CsePerson.Number;

                  break;
                case "CH":
                  export.SpDocKey.KeyChild = entities.CsePerson.Number;

                  break;
                default:
                  break;
              }
            }
          }
          else if (AsChar(import.AlternateMethod.Flag) == '3')
          {
            if (!ReadLegalAction3())
            {
              return;
            }

            if (!ReadTribunal())
            {
              return;
            }

            // mlb - PR00259407 - 01/13/2006 - The original read each statement 
            // is commented out
            // and replaced with a read each that also checks for the character 
            // string "MWONOTHC".
            // This is to allow the new one to be picked up whether or not the 
            // characters "MWO" are found.
            if (ReadLegalAction1())
            {
              if (ReadCsePersonLegalActionIncomeSource5())
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    export.SpDocKey.KeyAp = entities.CsePerson.Number;

                    break;
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }
              }
            }

            // end
          }
          else if (AsChar(import.AlternateMethod.Flag) == '4')
          {
            if (ReadCsePersonLegalActionIncomeSource3())
            {
              switch(TrimEnd(import.CaseRole.Type1))
              {
                case "AP":
                  export.SpDocKey.KeyAp = entities.CsePerson.Number;

                  break;
                case "AR":
                  export.SpDocKey.KeyAr = entities.CsePerson.Number;

                  break;
                case "CH":
                  export.SpDocKey.KeyChild = entities.CsePerson.Number;

                  break;
                default:
                  break;
              }
            }

            if (ReadCsePersonLegalActionPersonResource())
            {
              if (Lt(entities.LegalActionIncomeSource.EffectiveDate,
                entities.LegalActionPersonResource.EffectiveDate))
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    export.SpDocKey.KeyAp = entities.CsePerson.Number;

                    break;
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }
              }
            }
          }
          else if (AsChar(import.AlternateMethod.Flag) == '5')
          {
            // -----------------------------------------------------------------------
            // Rread AP from most recent IWGL attached to most recent IWO legal 
            // action.
            // Legal_Action_Income_Source.Withholding_Type <> spaces ==> IWO/MWO
            // record.
            // ---------------------------------------------------------------------
            if (!ReadLegalAction3())
            {
              return;
            }

            if (!ReadTribunal())
            {
              return;
            }

            foreach(var item in ReadLegalAction4())
            {
              if (!Equal(entities.FindApLegalAction.ActionTaken, "IWO") && !
                Equal(entities.FindApLegalAction.ActionTaken, "IWOMODO") && !
                Equal(entities.FindApLegalAction.ActionTaken, "IWONOTKS") && !
                Equal(entities.FindApLegalAction.ActionTaken, "IWONOTKM") && !
                Equal(entities.FindApLegalAction.ActionTaken, "ORDIWO2"))
              {
                continue;
              }

              if (ReadCsePersonLegalActionIncomeSource5())
              {
                switch(TrimEnd(import.CaseRole.Type1))
                {
                  case "AP":
                    export.SpDocKey.KeyAp = entities.CsePerson.Number;

                    break;
                  case "AR":
                    export.SpDocKey.KeyAr = entities.CsePerson.Number;

                    break;
                  case "CH":
                    export.SpDocKey.KeyChild = entities.CsePerson.Number;

                    break;
                  default:
                    break;
                }

                break;
              }
            }
          }
          else if (AsChar(import.AlternateMethod.Flag) == '6')
          {
            // -----------------------------------------------------------------------
            // For the following documents, read AP from the latest legal
            // action of class 'O' or 'J', that has a legal detail of type 'HIC
            // '.
            // ---------------------------------------------------------------------
            if (!ReadLegalAction3())
            {
              return;
            }

            if (!ReadTribunal())
            {
              return;
            }

            foreach(var item in ReadLegalAction5())
            {
              foreach(var item1 in ReadLegalActionDetail3())
              {
                if (ReadCsePerson1())
                {
                  switch(TrimEnd(import.CaseRole.Type1))
                  {
                    case "AP":
                      export.SpDocKey.KeyAp = entities.CsePerson.Number;

                      break;
                    case "AR":
                      export.SpDocKey.KeyAr = entities.CsePerson.Number;

                      break;
                    case "CH":
                      export.SpDocKey.KeyChild = entities.CsePerson.Number;

                      break;
                    default:
                      break;
                  }

                  goto ReadEach13;
                }
              }
            }

ReadEach13:
            ;
          }
          else if (ReadCsePerson2())
          {
            switch(TrimEnd(import.CaseRole.Type1))
            {
              case "AP":
                export.SpDocKey.KeyAp = entities.CsePerson.Number;

                break;
              case "AR":
                export.SpDocKey.KeyAr = entities.CsePerson.Number;

                break;
              case "CH":
                export.SpDocKey.KeyChild = entities.CsePerson.Number;

                break;
              default:
                break;
            }
          }

          break;
        case "OAA":
          // mjr---> Must be passed in
          // mjr---> Only applicable for PR
          break;
        case "OBL":
          // mjr---> Must be passed in
          // mjr---> Only applicable for PR
          // mjr
          // ---------------------------------------------------
          // 06/15/2000
          // PR# 97135 - Allow Org Name to be returned for Business Object 'OBL'
          // ----------------------------------------------------------------
          if (Equal(import.CaseRole.Type1, "PR"))
          {
            if (!IsEmpty(export.SpDocKey.KeyPerson))
            {
              if (ReadCsePerson18())
              {
                if (AsChar(entities.CsePerson.Type1) == 'O')
                {
                  export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
                  export.CsePerson.Number = entities.CsePerson.Number;
                  export.CsePersonsWorkSet.FirstName =
                    entities.CsePerson.OrganizationName ?? Spaces(12);

                  return;
                }
              }
              else
              {
                return;
              }
            }
          }
          else
          {
          }

          break;
        case "OVR":
          if (Equal(import.CaseRole.Type1, "PR"))
          {
            if (!IsEmpty(export.SpDocKey.KeyPerson))
            {
              break;
            }

            if (export.SpDocKey.KeyCashRcptDetail > 0 && export
              .SpDocKey.KeyCashRcptEvent > 0 && export
              .SpDocKey.KeyCashRcptSource > 0 && export
              .SpDocKey.KeyCashRcptType > 0)
            {
              if (ReadCashReceiptDetail())
              {
                export.SpDocKey.KeyPerson =
                  entities.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
              }
            }
          }
          else
          {
          }

          break;
        case "PER":
          // mjr---> Must be passed in
          break;
        case "PHI":
          if (export.SpDocKey.KeyHealthInsCoverage <= 0)
          {
            return;
          }

          // mjr
          // ----------------------------------------------
          // 01/11/1999
          // Next check sp_doc_key to determine if the user has
          // already selected the <AP,AR,CH or PR> they are using.
          // -----------------------------------------------------------
          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AP":
              if (!IsEmpty(export.SpDocKey.KeyAp))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 01/11/1999
              // No AP was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              break;
            case "AR":
              if (!IsEmpty(export.SpDocKey.KeyAr))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 01/11/1999
              // No AR was designated by the user.  Continue processing to
              // check for multiples.
              // -----------------------------------------------------------
              break;
            case "CH":
              if (!IsEmpty(export.SpDocKey.KeyChild))
              {
                goto Test;
              }

              // mjr
              // ----------------------------------------------
              // 01/11/1999
              // No CH was designated by the user.  Skip all fields
              // for this role_code.
              // -----------------------------------------------------------
              return;
            default:
              return;
          }

          if (Equal(import.CaseRole.Type1, "AP"))
          {
            if (ReadCsePerson15())
            {
              if (ReadContact())
              {
                export.CsePersonsWorkSet.Number = "HEALTHINS";
              }
              else
              {
                export.CsePersonsWorkSet.Number = entities.CsePerson.Number;
              }
            }
          }
          else
          {
            // mjr
            // ----------------------------------------------
            // 01/11/1999
            // Check for multiples of AR role code for the case.
            // -----------------------------------------------------------
            // Michael Quinn   
            // -------------------------------------------------------
            // 08/11/2004
            // If there is a case number in the import then use it to read the 
            // case.
            if (!IsEmpty(import.SpDocKey.KeyCase))
            {
              if (!ReadCase1())
              {
                return;
              }
            }
            else if (!ReadCase2())
            {
              return;
            }

            local.Count.Count = 0;

            foreach(var item in ReadCsePerson25())
            {
              ++local.Count.Count;
            }

            if (local.Count.Count < 1)
            {
              // mjr
              // ----------------------------------------------
              // 01/11/1999
              // No AR was found.  Skip all fields for this
              // role code
              // -----------------------------------------------------------
              return;
            }

            if (local.Count.Count > 1)
            {
              // mjr
              // ----------------------------------------------
              // 01/11/1999
              // Because there are multiples of <AP,AR or CH>, the user
              // needs to tell us which <AP,AR,CH or PR> they are using.
              // Skip all fields for this role code.
              // -----------------------------------------------------------
              return;
            }
          }

          // mjr
          // ----------------------------------------------
          // 01/11/1999
          // The case only has one <AP,AR,CH or PR>, so we use that one.
          // -----------------------------------------------------------
          switch(TrimEnd(import.CaseRole.Type1))
          {
            case "AP":
              export.SpDocKey.KeyAp = export.CsePersonsWorkSet.Number;

              break;
            case "AR":
              export.SpDocKey.KeyAr = entities.CsePerson.Number;

              break;
            default:
              break;
          }

          break;
        case "RCP":
          if (Equal(import.CaseRole.Type1, "PR"))
          {
            if (export.SpDocKey.KeyRecaptureRule > 0)
            {
              if (ReadCsePerson10())
              {
                export.SpDocKey.KeyPerson = entities.CsePerson.Number;
              }
            }
            else
            {
            }
          }
          else
          {
          }

          break;
        default:
          break;
      }

Test:

      switch(TrimEnd(import.CaseRole.Type1))
      {
        case "AP":
          export.CsePersonsWorkSet.Number = export.SpDocKey.KeyAp;

          break;
        case "AR":
          export.CsePersonsWorkSet.Number = export.SpDocKey.KeyAr;

          break;
        case "CH":
          export.CsePersonsWorkSet.Number = export.SpDocKey.KeyChild;

          break;
        case "PR":
          export.CsePersonsWorkSet.Number = export.SpDocKey.KeyPerson;

          break;
        default:
          break;
      }

      if (IsEmpty(export.CsePersonsWorkSet.Number))
      {
        return;
      }

      if (Equal(export.CsePersonsWorkSet.Number, "AAPPEAL"))
      {
        // mjr
        // ------------------------------------------------------
        // Move admin_appeal info to export views
        // ---------------------------------------------------------
        export.CsePersonsWorkSet.FirstName =
          entities.AdministrativeAppeal.AppellantFirstName ?? Spaces(12);
        export.CsePersonsWorkSet.MiddleInitial =
          entities.AdministrativeAppeal.AppellantMiddleInitial ?? Spaces(1);
        export.CsePersonsWorkSet.LastName =
          entities.AdministrativeAppeal.AppellantLastName ?? Spaces(17);
      }
      else if (Equal(export.CsePersonsWorkSet.Number, "INFO REQ"))
      {
        // mjr
        // ------------------------------------------------------
        // Move info_request info to export views
        // ---------------------------------------------------------
        export.CsePersonsWorkSet.FirstName =
          entities.InformationRequest.ApplicantFirstName ?? Spaces(12);
        export.CsePersonsWorkSet.MiddleInitial =
          entities.InformationRequest.ApplicantMiddleInitial ?? Spaces(1);
        export.CsePersonsWorkSet.LastName =
          entities.InformationRequest.ApplicantLastName ?? Spaces(17);
      }
      else if (Equal(export.CsePersonsWorkSet.Number, "HEALTHINS"))
      {
        // mjr
        // ------------------------------------------------------
        // Move contact info to export views
        // ---------------------------------------------------------
        export.CsePersonsWorkSet.FirstName = entities.Contact.NameFirst ?? Spaces
          (12);
        export.CsePersonsWorkSet.MiddleInitial =
          entities.Contact.MiddleInitial ?? Spaces(1);
        export.CsePersonsWorkSet.LastName = entities.Contact.NameLast ?? Spaces
          (17);
        export.Contact.CompanyName = entities.Contact.CompanyName;
      }
      else
      {
        if (AsChar(import.Batch.Flag) == 'Y')
        {
          UseSiReadCsePersonBatch1();

          if (!IsEmpty(local.Read.Type1) && Equal
            (local.Read.AdabasResponseCd, "0148"))
          {
            export.ErrorDocumentField.ScreenPrompt = "Resource Error";
            export.ErrorFieldValue.Value = "ADABAS Unavailable";
          }
        }
        else
        {
          UseSiReadCsePerson1();

          if (!IsEmpty(local.Read.Type1))
          {
            export.ErrorDocumentField.ScreenPrompt = "ABEND";
            export.ErrorFieldValue.Value = "SI_READ_CSE_PERSON";
          }
        }

        if (!IsEmpty(local.Read.Type1) || !IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.DateOfDeath = source.DateOfDeath;
    target.BirthPlaceState = source.BirthPlaceState;
    target.HomePhone = source.HomePhone;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.FamilyViolenceIndicator = source.FamilyViolenceIndicator;
    target.BirthplaceCountry = source.BirthplaceCountry;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Read.Assign(useExport.AbendData);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson(useExport.CsePerson, export.CsePerson);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Export1.Item.GcsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Read.Assign(useExport.AbendData);
    export.Export1.Update.GcsePersonsWorkSet.
      Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson(useExport.CsePerson, export.Export1.Update.GcsePerson);
  }

  private void UseSiReadCsePersonBatch1()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Read.Assign(useExport.AbendData);
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson(useExport.CsePerson, export.CsePerson);
  }

  private void UseSiReadCsePersonBatch2()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number =
      export.Export1.Item.GcsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.Read.Assign(useExport.AbendData);
    export.Export1.Update.GcsePersonsWorkSet.
      Assign(useExport.CsePersonsWorkSet);
    MoveCsePerson(useExport.CsePerson, export.Export1.Update.GcsePerson);
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "adminAppealId", export.SpDocKey.KeyAdminAppeal);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.AppellantLastName =
          db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.AppellantFirstName =
          db.GetNullableString(reader, 2);
        entities.AdministrativeAppeal.AppellantMiddleInitial =
          db.GetNullableString(reader, 3);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SpDocKey.KeyCase);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.SpDocKey.KeyChild);
        db.SetInt64(command, "hcvId", export.SpDocKey.KeyHealthInsCoverage);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
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
        db.SetString(command, "cspNumber", export.SpDocKey.KeyChild);
        db.SetInt64(command, "hcvId", export.SpDocKey.KeyHealthInsCoverage);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase3()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.SpDocKey.KeyChild);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase4()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.SpDocKey.KeyAppointment.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase5()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase5",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", export.SpDocKey.KeyInterstateRequest);
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase6()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase6",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "adminAppealId", export.SpDocKey.KeyAdminAppeal);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", export.SpDocKey.KeyCashRcptDetail);
        db.SetInt32(command, "crtIdentifier", export.SpDocKey.KeyCashRcptType);
        db.SetInt32(command, "crvIdentifier", export.SpDocKey.KeyCashRcptEvent);
        db.
          SetInt32(command, "cstIdentifier", export.SpDocKey.KeyCashRcptSource);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadContact()
  {
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.CsePerson.Number);
        db.
          SetInt64(command, "identifier", export.SpDocKey.KeyHealthInsCoverage);
          
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.CompanyName = db.GetNullableString(reader, 2);
        entities.Contact.NameLast = db.GetNullableString(reader, 3);
        entities.Contact.NameFirst = db.GetNullableString(reader, 4);
        entities.Contact.MiddleInitial = db.GetNullableString(reader, 5);
        entities.Contact.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.FindApLegalActionDetail.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", entities.FindApLegalAction.Identifier);
        db.SetNullableInt32(
          command, "ladRNumber", entities.FindApLegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier",
          entities.FindApLegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson10()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson10",
      (db, command) =>
      {
        db.
          SetInt32(command, "recaptureRuleId", export.SpDocKey.KeyRecaptureRule);
          
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson11()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson11",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.SpDocKey.KeyAppointment.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson12()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson12",
      (db, command) =>
      {
        db.
          SetInt32(command, "identifier", export.SpDocKey.KeyInterstateRequest);
          
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson13()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson13",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", export.SpDocKey.KeyGeneticTest);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson14()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson14",
      (db, command) =>
      {
        db.SetInt32(command, "testNumber", export.SpDocKey.KeyGeneticTest);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson15()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson15",
      (db, command) =>
      {
        db.
          SetInt64(command, "identifier", export.SpDocKey.KeyHealthInsCoverage);
          
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson16()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson16",
      (db, command) =>
      {
        db.SetInt32(command, "adminAppealId", export.SpDocKey.KeyAdminAppeal);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson17()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson17",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SpDocKey.KeyAr);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson18()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson18",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson19()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson19",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson20()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson20",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
        db.SetString(command, "croType", import.CaseRole.Type1);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson21()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson21",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
        db.SetNullableString(
          command, "accountType", local.LegalActionPerson.AccountType ?? "");
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson22()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson22",
      (db, command) =>
      {
        db.SetNullableString(
          command, "accountType", local.LegalActionPerson.AccountType ?? "");
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "ladRNumber", export.SpDocKey.KeyLegalActionDetail);
        db.SetNullableInt32(
          command, "lgaRIdentifier", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson23()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson23",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson24()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson24",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "fvLtrSentDt", local.Temp.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
        db.SetString(command, "type", import.CaseRole.Type1);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson25()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson25",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson26()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson26",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SpDocKey.KeyAp);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "numb", export.SpDocKey.KeyAp);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson5()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson5",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetString(command, "keyCase", export.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson6()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson6",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson7()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson7",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "testNumber", export.SpDocKey.KeyGeneticTest);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson8()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson8",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson9()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson9",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonLegalActionIncomeSource1()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadCsePersonLegalActionIncomeSource1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetString(command, "casNumber", export.SpDocKey.KeyCase);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 5);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 7);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 8);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 9);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 10);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 11);
        entities.CsePerson.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonLegalActionIncomeSource2()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadCsePersonLegalActionIncomeSource2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
        db.SetDate(
          command, "effectiveDt", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "type", import.CaseRole.Type1);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 5);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 7);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 8);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 9);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 10);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 11);
        entities.CsePerson.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonLegalActionIncomeSource3()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadCsePersonLegalActionIncomeSource3",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 5);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 7);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 8);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 9);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 10);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 11);
        entities.CsePerson.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonLegalActionIncomeSource4()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadCsePersonLegalActionIncomeSource4",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 5);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 7);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 8);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 9);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 10);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 11);
        entities.CsePerson.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonLegalActionIncomeSource5()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionIncomeSource.Populated = false;

    return Read("ReadCsePersonLegalActionIncomeSource5",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.FindApLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.LegalActionIncomeSource.CspNumber = db.GetString(reader, 5);
        entities.LegalActionIncomeSource.LgaIdentifier = db.GetInt32(reader, 6);
        entities.LegalActionIncomeSource.IsrIdentifier =
          db.GetDateTime(reader, 7);
        entities.LegalActionIncomeSource.EffectiveDate = db.GetDate(reader, 8);
        entities.LegalActionIncomeSource.WithholdingType =
          db.GetString(reader, 9);
        entities.LegalActionIncomeSource.EndDate =
          db.GetNullableDate(reader, 10);
        entities.LegalActionIncomeSource.Identifier = db.GetInt32(reader, 11);
        entities.CsePerson.Populated = true;
        entities.LegalActionIncomeSource.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonLegalActionPersonResource()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionPersonResource.Populated = false;

    return Read("ReadCsePersonLegalActionPersonResource",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 3);
        entities.CsePerson.FvLetterSentDate = db.GetNullableDate(reader, 4);
        entities.LegalActionPersonResource.CspNumber = db.GetString(reader, 5);
        entities.LegalActionPersonResource.CprResourceNo =
          db.GetInt32(reader, 6);
        entities.LegalActionPersonResource.LgaIdentifier =
          db.GetInt32(reader, 7);
        entities.LegalActionPersonResource.EffectiveDate =
          db.GetDate(reader, 8);
        entities.LegalActionPersonResource.LienType =
          db.GetNullableString(reader, 9);
        entities.LegalActionPersonResource.Identifier = db.GetInt32(reader, 10);
        entities.CsePerson.Populated = true;
        entities.LegalActionPersonResource.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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

  private bool ReadLegalAction1()
  {
    entities.FindApLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.FindApLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.FindApLegalAction.Classification = db.GetString(reader, 1);
        entities.FindApLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.FindApLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.FindApLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.FindApLegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.FindApLegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.FindApLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.FindApLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.FindApLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.FindApLegalAction.Classification = db.GetString(reader, 1);
        entities.FindApLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.FindApLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.FindApLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.FindApLegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.FindApLegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.FindApLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction4()
  {
    entities.FindApLegalAction.Populated = false;

    return ReadEach("ReadLegalAction4",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.FindApLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.FindApLegalAction.Classification = db.GetString(reader, 1);
        entities.FindApLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.FindApLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.FindApLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.FindApLegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.FindApLegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.FindApLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction5()
  {
    entities.FindApLegalAction.Populated = false;

    return ReadEach("ReadLegalAction5",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
        db.SetNullableString(
          command, "courtCaseNo", entities.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.FindApLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.FindApLegalAction.Classification = db.GetString(reader, 1);
        entities.FindApLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.FindApLegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.FindApLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.FindApLegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.FindApLegalAction.TrbId = db.GetNullableInt32(reader, 6);
        entities.FindApLegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail1()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail1",
      (db, command) =>
      {
        db.SetString(command, "code", local.ObligationType.Code);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 3);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 4);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail2()
  {
    entities.LegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId", entities.ObligationType.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", export.SpDocKey.KeyLegalAction);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 3);
        entities.LegalActionDetail.DetailType = db.GetString(reader, 4);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 5);
        entities.LegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail3()
  {
    entities.FindApLegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail3",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.FindApLegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.FindApLegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.FindApLegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.FindApLegalActionDetail.EffectiveDate = db.GetDate(reader, 2);
        entities.FindApLegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 3);
        entities.FindApLegalActionDetail.DetailType = db.GetString(reader, 4);
        entities.FindApLegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.FindApLegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", local.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
        entities.Tribunal.Identifier = db.GetInt32(reader, 1);
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
    /// <summary>A AlternateMethodsGroup group.</summary>
    [Serializable]
    public class AlternateMethodsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public ObligationType G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType g;
    }

    /// <summary>
    /// Gets a value of AlternateMethods.
    /// </summary>
    [JsonIgnore]
    public Array<AlternateMethodsGroup> AlternateMethods =>
      alternateMethods ??= new(AlternateMethodsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AlternateMethods for json serialization.
    /// </summary>
    [JsonPropertyName("alternateMethods")]
    [Computed]
    public IList<AlternateMethodsGroup> AlternateMethods_Json
    {
      get => alternateMethods;
      set => AlternateMethods.Assign(value);
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
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
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
    /// A value of ReturnGroup.
    /// </summary>
    [JsonPropertyName("returnGroup")]
    public Common ReturnGroup
    {
      get => returnGroup ??= new();
      set => returnGroup = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private Array<AlternateMethodsGroup> alternateMethods;
    private Common alternateMethod;
    private Common batch;
    private CaseRole caseRole;
    private Common returnGroup;
    private Document document;
    private DateWorkArea current;
    private SpDocKey spDocKey;
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
      /// A value of GcsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gcsePersonsWorkSet")]
      public CsePersonsWorkSet GcsePersonsWorkSet
      {
        get => gcsePersonsWorkSet ??= new();
        set => gcsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GcsePerson.
      /// </summary>
      [JsonPropertyName("gcsePerson")]
      public CsePerson GcsePerson
      {
        get => gcsePerson ??= new();
        set => gcsePerson = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonsWorkSet gcsePersonsWorkSet;
      private CsePerson gcsePerson;
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
    /// A value of ZdelExportErrorInd.
    /// </summary>
    [JsonPropertyName("zdelExportErrorInd")]
    public Common ZdelExportErrorInd
    {
      get => zdelExportErrorInd ??= new();
      set => zdelExportErrorInd = value;
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
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Array<ExportGroup> export1;
    private SpDocKey spDocKey;
    private Common zdelExportErrorInd;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
    private Contact contact;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public DateWorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of FoundArrears.
    /// </summary>
    [JsonPropertyName("foundArrears")]
    public Common FoundArrears
    {
      get => foundArrears ??= new();
      set => foundArrears = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of MultipleObligations.
    /// </summary>
    [JsonPropertyName("multipleObligations")]
    public Common MultipleObligations
    {
      get => multipleObligations ??= new();
      set => multipleObligations = value;
    }

    /// <summary>
    /// A value of FoundObligation.
    /// </summary>
    [JsonPropertyName("foundObligation")]
    public Common FoundObligation
    {
      get => foundObligation ??= new();
      set => foundObligation = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public AbendData Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private DateWorkArea temp;
    private Common foundArrears;
    private DateWorkArea current;
    private DateWorkArea null1;
    private ObligationType obligationType;
    private Common multipleObligations;
    private Common foundObligation;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private Common count;
    private AbendData read;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of FindApLegalAction.
    /// </summary>
    [JsonPropertyName("findApLegalAction")]
    public LegalAction FindApLegalAction
    {
      get => findApLegalAction ??= new();
      set => findApLegalAction = value;
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
    /// A value of FindApLegalActionDetail.
    /// </summary>
    [JsonPropertyName("findApLegalActionDetail")]
    public LegalActionDetail FindApLegalActionDetail
    {
      get => findApLegalActionDetail ??= new();
      set => findApLegalActionDetail = value;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
    }

    private Contact contact;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Appointment appointment;
    private LegalActionDetail legalActionDetail;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private AdministrativeAppeal administrativeAppeal;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CaseRole absentParent;
    private GeneticTest geneticTest;
    private CaseRole child;
    private InformationRequest informationRequest;
    private LegalActionIncomeSource legalActionIncomeSource;
    private IncomeSource incomeSource;
    private Tribunal tribunal;
    private LegalAction findApLegalAction;
    private LegalActionPersonResource legalActionPersonResource;
    private CsePersonResource csePersonResource;
    private LegalActionDetail findApLegalActionDetail;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private PersonalHealthInsurance personalHealthInsurance;
    private CsePersonAccount csePersonAccount;
    private RecaptureRule recaptureRule;
  }
#endregion
}
