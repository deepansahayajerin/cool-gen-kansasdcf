// Program: SP_B709_WRITE_DOCUMENT, ID: 374363798, model: 746.
// Short name: SWE02518
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B709_WRITE_DOCUMENT.
/// </summary>
[Serializable]
public partial class SpB709WriteDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B709_WRITE_DOCUMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB709WriteDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB709WriteDocument.
  /// </summary>
  public SpB709WriteDocument(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------
    // Description:  This CAB writes a KESSEP document based on a
    // template retrieved from a dataset, placing the field values at the
    // appropriate places.
    // ------------------------------------------------------------------------
    // Special characters in the template start with a backslash (\) character.
    // They are:
    //     \c - center line
    //     \d - start or end of a document definition
    //         must be followed by a colon (:), the document name,
    //         a period (.), and the document version
    //         may have a margin marker, a page length marker, and an output
    //         file marker
    //     \e - equation delimiter
    //         must be followed by a number from zero(0) to five(5)
    //         each number has a special meaning outlined below
    //     \f - field marker
    //         must be followed by a colon (:) and the field name
    //     \i - indent paragraph marker
    //      must be followed by a colon (:) and the indent number
    //      may be followed directly by a first line indent (\f) marker
    //     \l - field length specification
    //         if it exists, it must follow the field name in the field marker
    //         must be followed by a colon (:) and the length
    //     lines - page length marker
    //         if it exists, it must follow the version number in the document
    //         definition
    //         must be followed by a colon (:) and the lines number
    //     margin - margin marker
    //         if it exists, it must follow the version number in the document
    //         definition
    //         must be followed by a colon (:) and the margin number
    //     \n - new line
    // \p - new page
    //     \r - justification specification
    //         if it exists, it must follow the length in the length spec
    //     splitter - output file designation
    //      if it exists, it must follow the version number in the document
    //      definition
    //      must be followed by a period (.) and the output file number
    //     \t - tab (position)
    //         must be followed by a colon (:) and the position
    //     \\ - print a backslash character
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // Date		Developer	Request		Description
    // ------------------------------------------------------------------------
    // 02/07/2000	M Ramirez			Initial Development
    // 06/14/2000	M Ramirez	97196		Abends when data field
    // 						has a backslash in it.
    // 03/27/2001	M Ramirez	WR187 Seg H	last line not printed in
    // 						certain situations
    // 03/27/2001	M Ramirez	WR187 Seg H	corrected error with TAB
    // 09/13/2001	M Ashworth	PR 126727       Changed group view size from
    //                                                 
    // 30 to 50
    // 04/03/2002	M Ramirez	PR142886	Not printing 2nd SPLITTER page
    // ------------------------------------------------------------------------
    export.EabFileHandling.Status = "OK";

    if (IsEmpty(import.Document.Name))
    {
      export.EabFileHandling.Status = "DOCUMENT";
      ExitState = "DOCUMENT_NF";

      return;
    }

    if (import.Infrastructure.SystemGeneratedIdentifier <= 0)
    {
      export.EabFileHandling.Status = "INFRAST";
      ExitState = "INFRASTRUCTURE_NF";

      return;
    }

    if (ReadInfrastructure())
    {
      if (ReadOutgoingDocument())
      {
        local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
      }
      else
      {
        export.EabFileHandling.Status = "OUTDOCNF";

        return;
      }
    }
    else
    {
      export.EabFileHandling.Status = "INFRASNF";

      return;
    }

    // -----------------------------------------------------
    // Populate document templates from dataset
    // -----------------------------------------------------
    if (AsChar(local.TemplateFileRead.Flag) != 'Y')
    {
      local.TemplateFileRead.Flag = "N";
      local.EabFileHandling.Action = "OPEN";
      UseSpEabReadDocumentTemplate2();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        export.EabFileHandling.Status = "FAILOPEN";

        return;
      }

      local.EabFileHandling.Action = "READ";
      local.DocumentTemplates.Index = -1;

      do
      {
        UseSpEabReadDocumentTemplate1();

        if (Equal(export.EabFileHandling.Status, "OK"))
        {
          local.DocumentLineTemp.Text80 = ToUpper(local.DocumentLine.Text80);
          local.Position.Count = Find(local.DocumentLineTemp.Text80, "\\D:");

          if (local.Position.Count == 1)
          {
            local.NextPosition.Count = Find(local.DocumentLineTemp.Text80, ".");
            local.Document.Name =
              Substring(local.DocumentLineTemp.Text80, 4,
              local.NextPosition.Count - 4);
            local.Document.VersionNumber =
              Substring(local.DocumentLineTemp.Text80,
              local.NextPosition.Count + 1, 3);

            if (local.DocumentTemplates.Count > 0)
            {
              if (Equal(local.Document.Name, local.DocumentTemplates.Item.G.Name)
                && Equal
                (local.Document.VersionNumber,
                local.DocumentTemplates.Item.G.VersionNumber))
              {
                local.TemplateFileRead.Flag = "E";

                goto Test1;
              }
            }

            local.TemplateFileRead.Flag = "S";

            if (local.DocumentTemplates.Index + 2 > Local
              .DocumentTemplatesGroup.Capacity)
            {
              export.EabFileHandling.Status = "DOCTEMOF";

              return;
            }

            ++local.DocumentTemplates.Index;
            local.DocumentTemplates.CheckSize();

            local.DocumentTemplates.Item.SubDocumentLines.Index = -1;
            MoveDocument(local.Document, local.DocumentTemplates.Update.G);
            local.DocumentTemplates.Update.GlocalMarginWidth.Count = 0;
            local.DocumentTemplates.Update.GlocalPageMaxLines.Count = 0;
            local.Position.Count =
              Find(local.DocumentLineTemp.Text80, "MARGIN:");

            if (local.Position.Count > 0)
            {
              local.TempCalcCommon.ActionEntry =
                Substring(local.DocumentLineTemp.Text80, local.Position.Count +
                7, 2);

              if (!Lt(local.TempCalcCommon.ActionEntry, "00") && !
                Lt("99", local.TempCalcCommon.ActionEntry))
              {
                local.DocumentTemplates.Update.GlocalMarginWidth.Count =
                  (int)StringToNumber(local.TempCalcCommon.ActionEntry);
              }
            }

            if (local.DocumentTemplates.Item.GlocalMarginWidth.Count > 20)
            {
              local.DocumentTemplates.Update.GlocalMarginWidth.Count = 20;
            }
            else if (local.DocumentTemplates.Item.GlocalMarginWidth.Count <= 0)
            {
              local.DocumentTemplates.Update.GlocalMarginWidth.Count = 5;
            }
            else
            {
            }

            local.Position.Count =
              Find(local.DocumentLineTemp.Text80, "LINES:");

            if (local.Position.Count > 0)
            {
              local.TempCalcCommon.ActionEntry =
                Substring(local.DocumentLineTemp.Text80, local.Position.Count +
                6, 2);

              if (!Lt(local.TempCalcCommon.ActionEntry, "00") && !
                Lt("99", local.TempCalcCommon.ActionEntry))
              {
                local.DocumentTemplates.Update.GlocalPageMaxLines.Count =
                  (int)StringToNumber(local.TempCalcCommon.ActionEntry);
              }
            }

            if (local.DocumentTemplates.Item.GlocalPageMaxLines.Count > 55)
            {
              local.DocumentTemplates.Update.GlocalPageMaxLines.Count = 55;
            }
            else if (local.DocumentTemplates.Item.GlocalPageMaxLines.Count <= 0)
            {
              local.DocumentTemplates.Update.GlocalPageMaxLines.Count = 55;
            }
            else if (local.DocumentTemplates.Item.GlocalPageMaxLines.Count < 25)
            {
              local.DocumentTemplates.Update.GlocalPageMaxLines.Count = 25;
            }
            else
            {
            }

            local.Position.Count =
              Find(local.DocumentLineTemp.Text80, "SPLITTER.");

            if (local.Position.Count > 0)
            {
              local.TempCalcWorkArea.Text3 =
                Substring(local.DocumentLineTemp.Text80, local.Position.Count +
                9, 3);

              if (!Lt(local.TempCalcWorkArea.Text3, "000") && !
                Lt("999", local.TempCalcWorkArea.Text3))
              {
                local.TempCalcCommon.Count =
                  (int)StringToNumber(local.TempCalcWorkArea.Text3);

                if (local.TempCalcCommon.Count > 97)
                {
                  local.TempCalcCommon.Count = 97;
                }
              }
              else
              {
                // mjr
                // ---------------------------------------------
                // 02/21/2000
                // User could define a text version number in the future
                // ----------------------------------------------------------
                local.TempCalcCommon.Count = 0;
              }
            }
            else
            {
              local.TempCalcCommon.Count = 0;
            }

            if (local.TempCalcCommon.Count == 0)
            {
              local.DocumentTemplates.Update.GlocalReportNumber.ReportNumber =
                1;
            }
            else
            {
              local.DocumentTemplates.Update.GlocalReportNumber.ReportNumber =
                local.TempCalcCommon.Count;
            }
          }
          else if (AsChar(local.TemplateFileRead.Flag) == 'S')
          {
            if (local.DocumentTemplates.Item.SubDocumentLines.Index + 2 > Local
              .SubDocumentLinesGroup.Capacity)
            {
              export.EabFileHandling.Status = "LINTEMOF";

              return;
            }

            ++local.DocumentTemplates.Item.SubDocumentLines.Index;
            local.DocumentTemplates.Item.SubDocumentLines.CheckSize();

            local.DocumentTemplates.Update.SubDocumentLines.Update.
              GlocalDocumentLine.Text80 = local.DocumentLine.Text80;
          }
        }
        else if (Equal(export.EabFileHandling.Status, "EOF"))
        {
          local.TemplateFileRead.Flag = "Y";
        }
        else
        {
          export.EabFileHandling.Status = "FAILREAD";

          return;
        }

Test1:
        ;
      }
      while(!Equal(export.EabFileHandling.Status, "EOF"));

      local.EabFileHandling.Action = "CLOSE";
      UseSpEabReadDocumentTemplate2();

      if (!Equal(export.EabFileHandling.Status, "OK"))
      {
        export.EabFileHandling.Status = "FAILCLOS";

        return;
      }

      // -----------------------------------------------------
      // Write SPLITTER page, if available
      // -----------------------------------------------------
      local.DocumentTemplates.Index = 0;

      for(var limit = local.DocumentTemplates.Count; local
        .DocumentTemplates.Index < limit; ++local.DocumentTemplates.Index)
      {
        if (!local.DocumentTemplates.CheckSize())
        {
          break;
        }

        if (!Equal(local.DocumentTemplates.Item.G.Name, "SPLITTER"))
        {
          continue;
        }

        if (!Lt(local.DocumentTemplates.Item.G.VersionNumber, "000") && !
          Lt("999", local.DocumentTemplates.Item.G.VersionNumber))
        {
          local.EabReportSend.ReportNumber =
            (int)StringToNumber(local.DocumentTemplates.Item.G.VersionNumber);

          if (local.EabReportSend.ReportNumber > 97)
          {
            local.EabReportSend.ReportNumber = 97;
          }
        }
        else
        {
          // mjr
          // ---------------------------------------------
          // 02/21/2000
          // User could define a text version number in the future
          // ----------------------------------------------------------
          continue;
        }

        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.Command = "DETAIL";

        for(local.DocumentTemplates.Item.SubDocumentLines.Index = 0; local
          .DocumentTemplates.Item.SubDocumentLines.Index < local
          .DocumentTemplates.Item.SubDocumentLines.Count; ++
          local.DocumentTemplates.Item.SubDocumentLines.Index)
        {
          if (!local.DocumentTemplates.Item.SubDocumentLines.CheckSize())
          {
            break;
          }

          local.EabReportSend.RptDetail =
            local.DocumentTemplates.Item.SubDocumentLines.Item.
              GlocalDocumentLine.Text80;
          UseSpEabWriteDocument();

          if (!Equal(export.EabFileHandling.Status, "OK"))
          {
            export.EabFileHandling.Status = "FAILWRIT";

            return;
          }
        }

        local.DocumentTemplates.Item.SubDocumentLines.CheckIndex();

        // mjr
        // ----------------------------------------------------
        // 04/03/2002
        // PR142886 - Not printing 2nd SPLITTER page
        // Removed ESCAPE
        // -----------------------------------------------------------------
      }

      local.DocumentTemplates.CheckIndex();
    }

    // -----------------------------------------------------
    // Check for this document's template
    // -----------------------------------------------------
    for(local.DocumentTemplates.Index = 0; local.DocumentTemplates.Index < local
      .DocumentTemplates.Count; ++local.DocumentTemplates.Index)
    {
      if (!local.DocumentTemplates.CheckSize())
      {
        break;
      }

      if (Equal(local.DocumentTemplates.Item.G.Name, import.Document.Name) && Equal
        (local.DocumentTemplates.Item.G.VersionNumber,
        import.Document.VersionNumber))
      {
        break;
      }
    }

    local.DocumentTemplates.CheckIndex();

    if (!Equal(local.DocumentTemplates.Item.G.Name, import.Document.Name) || !
      Equal(local.DocumentTemplates.Item.G.VersionNumber,
      import.Document.VersionNumber))
    {
      export.EabFileHandling.Status = "DOCTEMNF";

      return;
    }

    local.RptDtlMaxWidth.Count = (int)(80 - (long)2 * local
      .DocumentTemplates.Item.GlocalMarginWidth.Count);

    // -----------------------------------------------------
    // Format document from document template
    // -----------------------------------------------------
    local.EabReportSend.Command = "NEWPAGE";
    local.EabReportSend.RptDetail = "";
    local.RptDtlWidth.Count = 0;
    local.DocumentLineRemain.Text80 = "";
    local.DocumentLineRemainWidth.Count = 0;
    local.DocmntLineAfterCc.Text80 = "";
    local.DocmntLineAfterCcWidth.Count = 0;
    local.DocmntLineOverflow.Text80 = "";
    local.DocmntLineOverflowWidth.Count = 0;
    local.MailingMachLinesIndent.Count = 11;
    local.MailingMachCharsIndent.Count = 12;
    local.IndentParagraph.Count = 0;
    local.NoSpace.Flag = "";

    if (local.DocumentTemplates.Item.GlocalMarginWidth.Count < local
      .MailingMachCharsIndent.Count)
    {
      local.MailingMachineMarkWidth.Count =
        local.MailingMachCharsIndent.Count - local
        .DocumentTemplates.Item.GlocalMarginWidth.Count;
    }
    else
    {
      local.MailingMachineMarkWidth.Count = 0;
    }

    local.RptDtlWidth.Count = local.MailingMachineMarkWidth.Count;
    local.Print.Index = -1;
    local.EquationNesting.Index = -1;
    local.EquationIgnoreLevel.Count = 0;

    for(local.DocumentTemplates.Item.SubDocumentLines.Index = 0; local
      .DocumentTemplates.Item.SubDocumentLines.Index < local
      .DocumentTemplates.Item.SubDocumentLines.Count; ++
      local.DocumentTemplates.Item.SubDocumentLines.Index)
    {
      if (!local.DocumentTemplates.Item.SubDocumentLines.CheckSize())
      {
        break;
      }

      local.DocumentLineRemain.Text80 =
        local.DocumentTemplates.Item.SubDocumentLines.Item.GlocalDocumentLine.
          Text80;
      local.DocumentLineRemainWidth.Count =
        Length(TrimEnd(local.DocumentLineRemain.Text80));
      local.DocumentLineOriginal.Text80 = local.DocumentLineRemain.Text80;

      // mjr
      // ----------------------------------------
      // 02/21/2000
      // Add one space at the end
      // -----------------------------------------------------
      if (Find(local.DocumentLineRemain.Text80, "\\") <= 0)
      {
        // mjr
        // ----------------------------------------
        // 03/27/2001
        // Added for a line that is already too long
        // -----------------------------------------------------
        if (local.DocumentLineRemainWidth.Count < local.RptDtlMaxWidth.Count)
        {
          ++local.DocumentLineRemainWidth.Count;
        }
      }

      local.CenterRptDtl.Flag = "N";

      do
      {
        local.Position.Count = Find(local.DocumentLineRemain.Text80, "\\");

        if (local.RptDtlWidth.Count + local.Position.Count - 1 > local
          .RptDtlMaxWidth.Count)
        {
          // mjr
          // ------------------------------------------------------
          // The special character will be pushed to the next
          // line anyway, so ignore it now.
          // ---------------------------------------------------------
          local.Position.Count = 0;
        }

        if (local.Position.Count < 1)
        {
          // mjr
          // -------------------------------------------------
          // If we are in the middle of an equation, make sure
          // we are supposed to be printing this line
          // ----------------------------------------------------
          if (local.EquationIgnoreLevel.Count > 0)
          {
            local.DocumentLineRemain.Text80 = "";
            local.DocumentLineRemainWidth.Count = 0;

            goto Test2;
          }
          else if (local.EquationNesting.Index >= 0)
          {
            if (AsChar(local.EquationResultToSkip.Flag) != AsChar
              (local.EquationNesting.Item.GlocalEquationResult.Flag) && !
              IsEmpty(local.EquationResultToSkip.Flag))
            {
              local.DocumentLineRemain.Text80 = "";
              local.DocumentLineRemainWidth.Count = 0;

              goto Test2;
            }
          }

          if (local.RptDtlWidth.Count + local.DocumentLineRemainWidth.Count < local
            .RptDtlMaxWidth.Count)
          {
            if (local.RptDtlWidth.Count > 0)
            {
              if (AsChar(local.NoSpace.Flag) != 'Y')
              {
                if (IsEmpty(Substring(local.DocumentLineRemain.Text80, 1, 1)))
                {
                  local.NoSpace.Flag = "Y";
                }
                else if (IsEmpty(Substring(
                  local.EabReportSend.RptDetail, local.RptDtlWidth.Count, 1)))
                {
                  local.NoSpace.Flag = "Y";
                }
                else
                {
                }
              }

              if (AsChar(local.NoSpace.Flag) == 'Y')
              {
                local.NoSpace.Flag = "";
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1,
                  local.RptDtlWidth.Count) + local.DocumentLineRemain.Text80;
                local.RptDtlWidth.Count += local.DocumentLineRemainWidth.Count;
              }
              else
              {
                local.EabReportSend.RptDetail =
                  Substring(local.EabReportSend.RptDetail,
                  EabReportSend.RptDetail_MaxLength, 1,
                  local.RptDtlWidth.Count) + " " + local
                  .DocumentLineRemain.Text80;
                local.RptDtlWidth.Count = local.RptDtlWidth.Count + local
                  .DocumentLineRemainWidth.Count + 1;
              }
            }
            else
            {
              local.EabReportSend.RptDetail = local.DocumentLineRemain.Text80;
              local.RptDtlWidth.Count = local.DocumentLineRemainWidth.Count;
            }

            local.DocumentLineRemain.Text80 = "";
            local.DocumentLineRemainWidth.Count = 0;

            if (local.RptDtlWidth.Count + 1 >= local.RptDtlMaxWidth.Count)
            {
              local.EabReportSend.RptDetail =
                Substring(local.Null1.Text20, WorkArea.Text20_MaxLength, 1,
                local.DocumentTemplates.Item.GlocalMarginWidth.Count) + local
                .EabReportSend.RptDetail;

              if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
              {
                export.EabFileHandling.Status = "PRTOVRFL";

                goto AfterCycle;
              }

              ++local.Print.Index;
              local.Print.CheckSize();

              MoveEabReportSend(local.EabReportSend,
                local.Print.Update.GlocalRptDtlLine);
              local.EabReportSend.RptDetail = "";
              local.RptDtlWidth.Count = 0;

              if (local.PageLinesCount.Count >= local
                .DocumentTemplates.Item.GlocalPageMaxLines.Count)
              {
                local.EabReportSend.Command = "NEWPAGE";
                local.PageLinesCount.Count = 0;
              }
              else
              {
                local.EabReportSend.Command = "DETAIL";
                ++local.PageLinesCount.Count;
              }

              if (local.PageLinesCount.Count < local
                .MailingMachLinesIndent.Count)
              {
                local.RptDtlWidth.Count = local.MailingMachineMarkWidth.Count;
              }

              // mjr
              // -----------------------------------------------------
              // 04/19/2000
              // If we are indenting and it was the first 9 lines of a page,
              // we subtracted out the mailing mark width.  Add it back
              // in now that we are past the first 9 lines.
              // ------------------------------------------------------------------
              if (local.PageLinesCount.Count == local
                .MailingMachLinesIndent.Count && local.IndentParagraph.Count > 0
                )
              {
                local.IndentParagraph.Count += local.MailingMachineMarkWidth.
                  Count;
              }

              if (local.IndentParagraph.Count > 0)
              {
                local.RptDtlWidth.Count += local.IndentParagraph.Count;
              }
            }
          }
          else
          {
            for(local.NextPosition.Count = local.RptDtlMaxWidth.Count - local
              .RptDtlWidth.Count; local.NextPosition.Count >= 1; local
              .NextPosition.Count += -1)
            {
              if (IsEmpty(Substring(
                local.DocumentLineRemain.Text80, local.NextPosition.Count, 1)))
              {
                break;
              }
            }

            if (local.NextPosition.Count > 0)
            {
              if (local.DocmntLineAfterCcWidth.Count > 0)
              {
                if (local.DocmntLineOverflowWidth.Count > 0)
                {
                  local.TempCalcCommon.Count =
                    local.DocumentLineRemainWidth.Count + local
                    .DocmntLineAfterCcWidth.Count + local
                    .DocmntLineOverflowWidth.Count - local.NextPosition.Count;

                  if (local.TempCalcCommon.Count > 160)
                  {
                    export.EabFileHandling.Status = "LOCSTORG";

                    goto AfterCycle;
                  }
                  else
                  {
                    // mjr
                    // ----------------------------------------------------------
                    // Combine after_cc and overflow and the new part
                    // into after_cc and overflow
                    // -------------------------------------------------------------
                    if (local.DocumentLineRemainWidth.Count + local
                      .DocmntLineAfterCcWidth.Count - local
                      .NextPosition.Count > 80)
                    {
                      if (local.DocmntLineAfterCcWidth.Count + local
                        .DocmntLineOverflowWidth.Count > 80)
                      {
                        // mjr
                        // ---------------------------------------------------
                        // After_cc will need to be split.  Be careful to not 
                        // split
                        // a special character.
                        // If no special characters exist, split anywhere.
                        // If a special character exists in the part that is to 
                        // be
                        // moved to overflow, disregard the special character.
                        // Otherwise...
                        // ------------------------------------------------------
                        local.TempCalcCommon.Subscript =
                          Find(local.DocmntLineAfterCc.Text80, "\\");
                        local.TempCalcCommon.Count =
                          local.DocmntLineAfterCcWidth.Count + local
                          .DocmntLineOverflowWidth.Count - 79;

                        if (local.TempCalcCommon.Subscript == 0 || local
                          .TempCalcCommon.Subscript >= local
                          .TempCalcCommon.Count)
                        {
                          local.DocmntLineOverflowWidth.Count = 80;
                          local.DocmntLineOverflow.Text80 =
                            Substring(local.DocmntLineAfterCc.Text80,
                            WorkArea.Text80_MaxLength,
                            local.TempCalcCommon.Count, 80 -
                            local.DocmntLineOverflowWidth.Count) + local
                            .DocmntLineOverflow.Text80;
                          local.DocmntLineAfterCcWidth.Count =
                            local.DocumentLineRemainWidth.Count + local
                            .TempCalcCommon.Count - local.NextPosition.Count;
                          local.DocmntLineAfterCc.Text80 =
                            Substring(local.DocumentLineRemain.Text80,
                            WorkArea.Text80_MaxLength,
                            local.NextPosition.Count +
                            1, local.DocumentLineRemainWidth.Count -
                            local.NextPosition.Count) + Substring
                            (local.DocmntLineAfterCc.Text80,
                            WorkArea.Text80_MaxLength, 1,
                            local.TempCalcCommon.Count - 1);
                        }
                        else
                        {
                          export.EabFileHandling.Status = "LOCSTORG";

                          goto AfterCycle;
                        }
                      }
                      else
                      {
                        local.DocmntLineOverflowWidth.Count =
                          local.DocmntLineAfterCcWidth.Count + local
                          .DocmntLineOverflowWidth.Count;
                        local.DocmntLineOverflow.Text80 =
                          Substring(local.DocmntLineAfterCc.Text80,
                          WorkArea.Text80_MaxLength, 1,
                          local.DocmntLineAfterCcWidth.Count) + local
                          .DocmntLineOverflow.Text80;
                        local.DocmntLineAfterCcWidth.Count =
                          local.DocumentLineRemainWidth.Count - local
                          .NextPosition.Count;
                        local.DocmntLineAfterCc.Text80 =
                          Substring(local.DocumentLineRemain.Text80,
                          local.NextPosition.Count +
                          1, local.DocmntLineAfterCcWidth.Count);
                      }
                    }
                    else
                    {
                      local.DocmntLineAfterCcWidth.Count =
                        local.DocumentLineRemainWidth.Count + local
                        .DocmntLineAfterCcWidth.Count - local
                        .NextPosition.Count;
                      local.DocmntLineAfterCc.Text80 =
                        Substring(local.DocumentLineRemain.Text80,
                        WorkArea.Text80_MaxLength, local.NextPosition.Count +
                        1, local.DocumentLineRemainWidth.Count -
                        local.NextPosition.Count) + local
                        .DocmntLineAfterCc.Text80;
                    }
                  }
                }
                else
                {
                  local.DocmntLineOverflowWidth.Count =
                    local.DocmntLineAfterCcWidth.Count;
                  local.DocmntLineOverflow.Text80 =
                    local.DocmntLineAfterCc.Text80;
                  local.DocmntLineAfterCcWidth.Count =
                    local.DocumentLineRemainWidth.Count - local
                    .NextPosition.Count;
                  local.DocmntLineAfterCc.Text80 =
                    Substring(local.DocumentLineRemain.Text80,
                    local.NextPosition.Count +
                    1, local.DocmntLineAfterCcWidth.Count);
                }
              }
              else
              {
                local.DocmntLineAfterCcWidth.Count =
                  local.DocumentLineRemainWidth.Count - local
                  .NextPosition.Count;
                local.DocmntLineAfterCc.Text80 =
                  Substring(local.DocumentLineRemain.Text80,
                  local.NextPosition.Count +
                  1, local.DocmntLineAfterCcWidth.Count);
              }

              local.DocumentLineRemainWidth.Count = local.NextPosition.Count - 1
                ;
              local.DocumentLineRemain.Text80 =
                Substring(local.DocumentLineRemain.Text80, 1,
                local.DocumentLineRemainWidth.Count);
            }
            else
            {
              local.TempCalcCommon.Count =
                Find(String(
                  local.DocumentLineRemain.Text80, WorkArea.Text80_MaxLength),
                " ");

              if ((local.TempCalcCommon.Count <= 0 || local
                .TempCalcCommon.Count > local
                .DocumentLineRemainWidth.Count) && local
                .DocumentLineRemainWidth.Count > local.RptDtlMaxWidth.Count)
              {
                // mjr
                // -----------------------------------------------
                // 02/23/2000
                // Line cannot be split since there are no spaces nor hypens
                // Add a hypen at the end of the line and split it there.
                // ------------------------------------------------------------
                local.NextPosition.Count = local.RptDtlMaxWidth.Count - local
                  .RptDtlWidth.Count - 2;

                if (local.DocmntLineAfterCcWidth.Count > 0)
                {
                  if (local.DocmntLineOverflowWidth.Count > 0)
                  {
                    local.TempCalcCommon.Count =
                      local.DocumentLineRemainWidth.Count + local
                      .DocmntLineAfterCcWidth.Count + local
                      .DocmntLineOverflowWidth.Count - local
                      .NextPosition.Count;

                    if (local.TempCalcCommon.Count > 160)
                    {
                      export.EabFileHandling.Status = "LOCSTORG";

                      goto AfterCycle;
                    }
                    else
                    {
                      // mjr
                      // ----------------------------------------------------------
                      // Combine after_cc and overflow and the new part
                      // into after_cc and overflow
                      // -------------------------------------------------------------
                      if (local.DocumentLineRemainWidth.Count + local
                        .DocmntLineAfterCcWidth.Count - local
                        .NextPosition.Count > 80)
                      {
                        if (local.DocmntLineAfterCcWidth.Count + local
                          .DocmntLineOverflowWidth.Count > 80)
                        {
                          // mjr
                          // ---------------------------------------------------
                          // After_cc will need to be split.  Be careful to not 
                          // split
                          // a special character.
                          // If no special characters exist, split anywhere.
                          // If a special character exists in the part that is 
                          // to be
                          // moved to overflow, disregard the special character.
                          // Otherwise...
                          // ------------------------------------------------------
                          local.TempCalcCommon.Subscript =
                            Find(local.DocmntLineAfterCc.Text80, "\\");
                          local.TempCalcCommon.Count =
                            local.DocmntLineAfterCcWidth.Count + local
                            .DocmntLineOverflowWidth.Count - 79;

                          if (local.TempCalcCommon.Subscript == 0 || local
                            .TempCalcCommon.Subscript >= local
                            .TempCalcCommon.Count)
                          {
                            local.DocmntLineOverflowWidth.Count = 80;
                            local.DocmntLineOverflow.Text80 =
                              Substring(local.DocmntLineAfterCc.Text80,
                              WorkArea.Text80_MaxLength,
                              local.TempCalcCommon.Count, 80 -
                              local.DocmntLineOverflowWidth.Count) + local
                              .DocmntLineOverflow.Text80;
                            local.DocmntLineAfterCcWidth.Count =
                              local.DocumentLineRemainWidth.Count + local
                              .TempCalcCommon.Count - local.NextPosition.Count;
                            local.DocmntLineAfterCc.Text80 =
                              Substring(local.DocumentLineRemain.Text80,
                              WorkArea.Text80_MaxLength,
                              local.NextPosition.Count +
                              1, local.DocumentLineRemainWidth.Count -
                              local.NextPosition.Count) + Substring
                              (local.DocmntLineAfterCc.Text80,
                              WorkArea.Text80_MaxLength, 1,
                              local.TempCalcCommon.Count - 1);
                          }
                          else
                          {
                            export.EabFileHandling.Status = "LOCSTORG";

                            goto AfterCycle;
                          }
                        }
                        else
                        {
                          local.DocmntLineOverflowWidth.Count =
                            local.DocmntLineAfterCcWidth.Count + local
                            .DocmntLineOverflowWidth.Count;
                          local.DocmntLineOverflow.Text80 =
                            Substring(local.DocmntLineAfterCc.Text80,
                            WorkArea.Text80_MaxLength, 1,
                            local.DocmntLineAfterCcWidth.Count) + local
                            .DocmntLineOverflow.Text80;
                          local.DocmntLineAfterCcWidth.Count =
                            local.DocumentLineRemainWidth.Count - local
                            .NextPosition.Count;
                          local.DocmntLineAfterCc.Text80 =
                            Substring(local.DocumentLineRemain.Text80,
                            local.NextPosition.Count +
                            1, local.DocmntLineAfterCcWidth.Count);
                        }
                      }
                      else
                      {
                        local.DocmntLineAfterCcWidth.Count =
                          local.DocumentLineRemainWidth.Count + local
                          .DocmntLineAfterCcWidth.Count - local
                          .NextPosition.Count;
                        local.DocmntLineAfterCc.Text80 =
                          Substring(local.DocumentLineRemain.Text80,
                          WorkArea.Text80_MaxLength, local.NextPosition.Count +
                          1, local.DocumentLineRemainWidth.Count -
                          local.NextPosition.Count) + local
                          .DocmntLineAfterCc.Text80;
                      }
                    }
                  }
                  else
                  {
                    local.DocmntLineOverflowWidth.Count =
                      local.DocmntLineAfterCcWidth.Count;
                    local.DocmntLineOverflow.Text80 =
                      local.DocmntLineAfterCc.Text80;
                    local.DocmntLineAfterCcWidth.Count =
                      local.DocumentLineRemainWidth.Count - local
                      .NextPosition.Count;
                    local.DocmntLineAfterCc.Text80 =
                      Substring(local.DocumentLineRemain.Text80,
                      local.NextPosition.Count +
                      1, local.DocmntLineAfterCcWidth.Count);
                  }
                }
                else
                {
                  local.DocmntLineAfterCcWidth.Count =
                    local.DocumentLineRemainWidth.Count - local
                    .NextPosition.Count;
                  local.DocmntLineAfterCc.Text80 =
                    Substring(local.DocumentLineRemain.Text80,
                    local.NextPosition.Count +
                    1, local.DocmntLineAfterCcWidth.Count);
                }

                local.DocumentLineRemainWidth.Count =
                  local.NextPosition.Count + 1;
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80,
                  WorkArea.Text80_MaxLength, 1,
                  local.DocumentLineRemainWidth.Count - 1) + "-";
              }
              else
              {
                // mjr
                // -----------------------------------------------
                // 02/24/2000
                // The first word on the next line is too long to be
                // added to the current line (and is shorter than max_width).
                // Print the current line now.
                // ------------------------------------------------------------
                if (AsChar(local.CenterRptDtl.Flag) == 'Y')
                {
                  local.TempCalcCommon.Count =
                    (80 - local.RptDtlWidth.Count) / 2;
                  local.CenterRptDtl.Flag = "N";
                  local.EabReportSend.RptDetail =
                    Substring(local.Null1.Text40, WorkArea.Text40_MaxLength, 1,
                    local.TempCalcCommon.Count) + local
                    .EabReportSend.RptDetail;
                }
                else
                {
                  local.EabReportSend.RptDetail =
                    Substring(local.Null1.Text20, WorkArea.Text20_MaxLength, 1,
                    local.DocumentTemplates.Item.GlocalMarginWidth.Count) + local
                    .EabReportSend.RptDetail;
                }

                if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
                {
                  export.EabFileHandling.Status = "PRTOVRFL";

                  goto AfterCycle;
                }

                ++local.Print.Index;
                local.Print.CheckSize();

                MoveEabReportSend(local.EabReportSend,
                  local.Print.Update.GlocalRptDtlLine);
                local.EabReportSend.RptDetail = "";
                local.RptDtlWidth.Count = 0;

                if (local.PageLinesCount.Count >= local
                  .DocumentTemplates.Item.GlocalPageMaxLines.Count)
                {
                  local.EabReportSend.Command = "NEWPAGE";
                  local.PageLinesCount.Count = 0;
                }
                else
                {
                  local.EabReportSend.Command = "DETAIL";
                  ++local.PageLinesCount.Count;
                }

                if (local.PageLinesCount.Count < local
                  .MailingMachLinesIndent.Count)
                {
                  local.RptDtlWidth.Count = local.MailingMachineMarkWidth.Count;
                }

                // mjr
                // -----------------------------------------------------
                // 04/19/2000
                // If we are indenting and it was the first 9 lines of a page,
                // we subtracted out the mailing mark width.  Add it back
                // in now that we are past the first 9 lines.
                // ------------------------------------------------------------------
                if (local.PageLinesCount.Count == local
                  .MailingMachLinesIndent.Count && local
                  .IndentParagraph.Count > 0)
                {
                  local.IndentParagraph.Count += local.MailingMachineMarkWidth.
                    Count;
                }

                if (local.IndentParagraph.Count > 0)
                {
                  local.RptDtlWidth.Count += local.IndentParagraph.Count;
                }

                local.NoSpace.Flag = "";
              }
            }
          }

          local.DocumentLineTemp.Text80 =
            ToUpper(local.DocmntLineAfterCc.Text80);

          if (IsEmpty(local.EabReportSend.RptDetail) && local
            .DocumentLineRemainWidth.Count == 0 && Equal
            (local.DocumentLineTemp.Text80, "\\N"))
          {
            // mjr
            // ----------------------------------------------------------
            // We wrote a line and the only thing left is \n
            // This is an overflow that was intended to end the previous line.
            // Remove it now.
            // --------------------------------------------------------------
            local.DocmntLineAfterCc.Text80 = "";
            local.DocmntLineAfterCcWidth.Count = 0;
            local.IndentParagraph.Count = 0;
          }
        }
        else if (local.Position.Count > 1)
        {
          if (local.DocmntLineAfterCcWidth.Count > 0)
          {
            if (local.DocmntLineOverflowWidth.Count > 0)
            {
              local.TempCalcCommon.Count =
                local.DocumentLineRemainWidth.Count + local
                .DocmntLineAfterCcWidth.Count + local
                .DocmntLineOverflowWidth.Count + 1 - local.Position.Count;

              if (local.TempCalcCommon.Count > 160)
              {
                export.EabFileHandling.Status = "LOCSTORG";

                goto AfterCycle;
              }
              else
              {
                // mjr
                // ----------------------------------------------------------
                // Combine after_cc and overflow and the new part
                // into after_cc and overflow
                // -------------------------------------------------------------
                if (local.DocumentLineRemainWidth.Count + local
                  .DocmntLineAfterCcWidth.Count + 1 - local.Position.Count > 80
                  )
                {
                  if (local.DocmntLineAfterCcWidth.Count + local
                    .DocmntLineOverflowWidth.Count > 80)
                  {
                    // mjr
                    // ---------------------------------------------------
                    // After_cc will need to be split.  Be careful to not split
                    // a special character.
                    // If no special characters exist, split anywhere.
                    // If a special character exists in the part that is to be
                    // moved to overflow, disregard the special character.
                    // Otherwise...
                    // ------------------------------------------------------
                    local.TempCalcCommon.Subscript =
                      Find(local.DocmntLineAfterCc.Text80, "\\");
                    local.TempCalcCommon.Count =
                      local.DocmntLineAfterCcWidth.Count + local
                      .DocmntLineOverflowWidth.Count - 79;

                    if (local.TempCalcCommon.Subscript == 0 || local
                      .TempCalcCommon.Subscript >= local.TempCalcCommon.Count)
                    {
                      local.DocmntLineOverflowWidth.Count = 80;
                      local.DocmntLineOverflow.Text80 =
                        Substring(local.DocmntLineAfterCc.Text80,
                        WorkArea.Text80_MaxLength, local.TempCalcCommon.Count,
                        80 - local.DocmntLineOverflowWidth.Count) + local
                        .DocmntLineOverflow.Text80;
                      local.DocmntLineAfterCcWidth.Count =
                        local.DocumentLineRemainWidth.Count + local
                        .TempCalcCommon.Count - local.Position.Count;
                      local.DocmntLineAfterCc.Text80 =
                        Substring(local.DocumentLineRemain.Text80,
                        WorkArea.Text80_MaxLength, local.Position.Count,
                        local.DocumentLineRemainWidth.Count - local
                        .Position.Count + 1) + Substring
                        (local.DocmntLineAfterCc.Text80,
                        WorkArea.Text80_MaxLength, 1,
                        local.TempCalcCommon.Count - 1);
                    }
                    else
                    {
                      export.EabFileHandling.Status = "LOCSTORG";

                      goto AfterCycle;
                    }
                  }
                  else
                  {
                    local.DocmntLineOverflowWidth.Count =
                      local.DocmntLineAfterCcWidth.Count + local
                      .DocmntLineOverflowWidth.Count;
                    local.DocmntLineOverflow.Text80 =
                      Substring(local.DocmntLineAfterCc.Text80,
                      WorkArea.Text80_MaxLength, 1,
                      local.DocmntLineAfterCcWidth.Count) + local
                      .DocmntLineOverflow.Text80;
                    local.DocmntLineAfterCcWidth.Count =
                      local.DocumentLineRemainWidth.Count - local
                      .Position.Count + 1;
                    local.DocmntLineAfterCc.Text80 =
                      Substring(local.DocumentLineRemain.Text80,
                      local.Position.Count, local.DocmntLineAfterCcWidth.Count);
                      
                  }
                }
                else
                {
                  local.DocmntLineAfterCcWidth.Count =
                    local.DocumentLineRemainWidth.Count + local
                    .DocmntLineAfterCcWidth.Count - local.Position.Count + 1;
                  local.DocmntLineAfterCc.Text80 =
                    Substring(local.DocumentLineRemain.Text80,
                    WorkArea.Text80_MaxLength, local.Position.Count,
                    local.DocumentLineRemainWidth.Count - local
                    .Position.Count + 1) + local.DocmntLineAfterCc.Text80;
                }
              }
            }
            else
            {
              local.DocmntLineOverflowWidth.Count =
                local.DocmntLineAfterCcWidth.Count;
              local.DocmntLineOverflow.Text80 = local.DocmntLineAfterCc.Text80;
              local.DocmntLineAfterCcWidth.Count =
                local.DocumentLineRemainWidth.Count - local.Position.Count + 1;
              local.DocmntLineAfterCc.Text80 =
                Substring(local.DocumentLineRemain.Text80, local.Position.Count,
                local.DocmntLineAfterCcWidth.Count);
            }
          }
          else
          {
            local.DocmntLineAfterCcWidth.Count =
              local.DocumentLineRemainWidth.Count + 1 - local.Position.Count;
            local.DocmntLineAfterCc.Text80 =
              Substring(local.DocumentLineRemain.Text80, local.Position.Count,
              local.DocmntLineAfterCcWidth.Count);
          }

          local.DocumentLineRemainWidth.Count = local.Position.Count - 1;
          local.DocumentLineRemain.Text80 =
            Substring(local.DocumentLineRemain.Text80, 1,
            local.DocumentLineRemainWidth.Count);
        }
        else
        {
          // -----------------------------------------------------
          // Possible special characters:
          // 	\c - center line
          // 	\e - equation
          // 	\f - field
          // 	\i - indent
          // 	\n - new line
          // 	\p - new page
          // 	\t - tab (position)
          // 	\f - field marker
          // 		must be followed by a colon (:) and the field name
          // 	\\ - print a backslash character
          // (\d should not be placed in the document lines group)
          // -----------------------------------------------------
          local.SpecialCharacter.Text1 =
            Substring(local.DocumentLineRemain.Text80, 2, 1);
          local.SpecialCharacter.Text1 = ToUpper(local.SpecialCharacter.Text1);

          // mjr
          // -------------------------------------------------
          // If we are in the middle of an equation, make sure
          // we are supposed to be printing this line
          // Ignore all special characters except E
          // ----------------------------------------------------
          if (AsChar(local.SpecialCharacter.Text1) != 'E')
          {
            if (local.EquationIgnoreLevel.Count > 0)
            {
              local.DocumentLineTemp.Text80 =
                ToUpper(local.DocumentLineRemain.Text80);
              local.NextPosition.Count =
                Find(local.DocumentLineTemp.Text80, "\\E");

              if (local.NextPosition.Count > 0)
              {
                local.DocumentLineRemainWidth.Count =
                  local.DocumentLineRemainWidth.Count - local
                  .NextPosition.Count + 1;
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80,
                  local.NextPosition.Count,
                  local.DocumentLineRemainWidth.Count);
              }
              else
              {
                local.DocumentLineRemain.Text80 = "";
                local.DocumentLineRemainWidth.Count = 0;
              }

              goto Test2;
            }
            else if (local.EquationNesting.Index >= 0)
            {
              if (AsChar(local.EquationResultToSkip.Flag) != AsChar
                (local.EquationNesting.Item.GlocalEquationResult.Flag) && !
                IsEmpty(local.EquationResultToSkip.Flag))
              {
                local.DocumentLineTemp.Text80 =
                  ToUpper(local.DocumentLineRemain.Text80);
                local.NextPosition.Count =
                  Find(local.DocumentLineTemp.Text80, "\\E");

                if (local.NextPosition.Count > 0)
                {
                  local.DocumentLineRemainWidth.Count =
                    local.DocumentLineRemainWidth.Count - local
                    .NextPosition.Count + 1;
                  local.DocumentLineRemain.Text80 =
                    Substring(local.DocumentLineRemain.Text80,
                    local.NextPosition.Count,
                    local.DocumentLineRemainWidth.Count);
                }
                else
                {
                  local.DocumentLineRemain.Text80 = "";
                  local.DocumentLineRemainWidth.Count = 0;
                }

                goto Test2;
              }
            }
          }

          switch(AsChar(local.SpecialCharacter.Text1))
          {
            case 'C':
              // -------------------------------------------------
              // Center Marker
              // -------------------------------------------------
              // ---------------------------------------------------------
              // Special character must be the first non-space character on
              // the line
              // ---------------------------------------------------------
              local.NextPosition.Count =
                Find(local.DocumentLineOriginal.Text80, "\\C");
              local.DocumentLineTemp.Text80 =
                Substring(local.DocumentLineOriginal.Text80, 1,
                local.NextPosition.Count - 1);

              if (IsEmpty(local.DocumentLineTemp.Text80))
              {
                local.CenterRptDtl.Flag = "Y";
              }

              // ---------------------------------------------------
              // Remove \c from local_document_line_remain
              // ---------------------------------------------------
              local.DocumentLineRemainWidth.Count -= 2;
              local.DocumentLineRemain.Text80 =
                Substring(local.DocumentLineRemain.Text80, 3,
                local.DocumentLineRemainWidth.Count);

              // mjr
              // ----------------------------------------------------------------
              // End paragraph indenting, as the paragraph has ended
              // -------------------------------------------------------------------
              local.IndentParagraph.Count = 0;

              break;
            case 'E':
              // -------------------------------------------------
              // Equation Marker
              // -------------------------------------------------
              local.SpecialCharacter.Text1 =
                Substring(local.DocumentLineRemain.Text80, 3, 1);

              // ---------------------------------------------------
              // Remove \eN from local_document_line_remain
              // N is 0, 1, 2, 3, 4 or 5
              // ---------------------------------------------------
              local.DocumentLineRemainWidth.Count -= 3;

              if (local.DocumentLineRemainWidth.Count > 0)
              {
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80, 4,
                  local.DocumentLineRemainWidth.Count);
              }
              else
              {
                local.DocumentLineRemain.Text80 = "";
              }

              // mjr
              // ---------------------------------------------------
              // All portions of the equation are determined
              // Now evaluate for the result.
              // ------------------------------------------------------
              if (!IsEmpty(local.EquationOperand1.Text20) && !
                IsEmpty(local.EquationOperand2.Text20) && !
                IsEmpty(local.EquationOperator.ActionEntry))
              {
                if (Equal(local.EquationOperand1.Text20, "SPACES"))
                {
                  local.EquationOperand1.Text20 = "";
                }

                if (Equal(local.EquationOperand2.Text20, "SPACES"))
                {
                  local.EquationOperand2.Text20 = "";
                }

                switch(TrimEnd(local.EquationOperator.ActionEntry))
                {
                  case "EQ":
                    if (Equal(local.EquationOperand1.Text20,
                      local.EquationOperand2.Text20))
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "T";
                    }
                    else
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "F";
                    }

                    break;
                  case "GE":
                    if (!Lt(local.EquationOperand1.Text20,
                      local.EquationOperand2.Text20))
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "T";
                    }
                    else
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "F";
                    }

                    break;
                  case "GT":
                    if (Lt(local.EquationOperand2.Text20,
                      local.EquationOperand1.Text20))
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "T";
                    }
                    else
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "F";
                    }

                    break;
                  case "LE":
                    if (!Lt(local.EquationOperand2.Text20,
                      local.EquationOperand1.Text20))
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "T";
                    }
                    else
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "F";
                    }

                    break;
                  case "LT":
                    if (Lt(local.EquationOperand1.Text20,
                      local.EquationOperand2.Text20))
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "T";
                    }
                    else
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "F";
                    }

                    break;
                  case "NE":
                    if (!Equal(local.EquationOperand1.Text20,
                      local.EquationOperand2.Text20))
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "T";
                    }
                    else
                    {
                      local.EquationNesting.Update.GlocalEquationResult.Flag =
                        "F";
                    }

                    break;
                  default:
                    export.EabFileHandling.Status = "OPERATOR";

                    goto AfterCycle;
                }

                local.EquationOperand1.Text20 = "";
                local.EquationOperand2.Text20 = "";
                local.EquationOperator.ActionEntry = "";
              }

              switch(AsChar(local.SpecialCharacter.Text1))
              {
                case '0':
                  if (IsEmpty(local.EquationResultToSkip.Flag))
                  {
                    // mjr
                    // ------------------------------
                    // Set subscript
                    // ---------------------------------
                    if (local.EquationNesting.Index + 2 > Local
                      .EquationNestingGroup.Capacity)
                    {
                      export.EabFileHandling.Status = "NSTOVRFL";

                      goto AfterCycle;
                    }

                    ++local.EquationNesting.Index;
                    local.EquationNesting.CheckSize();

                    // mjr
                    // ------------------------------
                    // Set operand1
                    // ---------------------------------
                    local.DocumentLineTemp.Text80 =
                      ToUpper(local.DocumentLineRemain.Text80);
                    local.NextPosition.Count =
                      Find(local.DocumentLineTemp.Text80, "\\E");

                    if (local.NextPosition.Count <= 0)
                    {
                      export.EabFileHandling.Status = "OP1LNGTH";

                      goto AfterCycle;
                    }

                    local.EquationOperand1.Text20 =
                      Substring(local.DocumentLineRemain.Text80, 1,
                      local.NextPosition.Count - 1);
                    local.TempCalcCommon.ActionEntry =
                      Substring(local.EquationOperand1.Text20, 1, 2);
                    local.TempCalcCommon.ActionEntry =
                      ToUpper(local.TempCalcCommon.ActionEntry);

                    if (Equal(local.TempCalcCommon.ActionEntry, "\\F"))
                    {
                      local.EquationOperand.Count = 1;
                    }
                    else
                    {
                      // ---------------------------------------------------
                      // Remove operand1 from local_document_line_remain
                      // ---------------------------------------------------
                      local.DocumentLineRemainWidth.Count =
                        local.DocumentLineRemainWidth.Count - local
                        .NextPosition.Count + 1;

                      if (local.DocumentLineRemainWidth.Count > 0)
                      {
                        local.DocumentLineRemain.Text80 =
                          Substring(local.DocumentLineRemain.Text80,
                          local.NextPosition.Count,
                          local.DocumentLineRemainWidth.Count);
                      }
                      else
                      {
                        local.DocumentLineRemain.Text80 = "";
                      }
                    }
                  }
                  else
                  {
                    ++local.EquationIgnoreLevel.Count;
                  }

                  break;
                case '1':
                  if (local.EquationIgnoreLevel.Count == 0)
                  {
                    local.DocumentLineTemp.Text80 =
                      ToUpper(local.DocumentLineRemain.Text80);
                    local.EquationOperator.ActionEntry =
                      Substring(local.DocumentLineRemain.Text80, 1, 2);

                    switch(TrimEnd(local.EquationOperator.ActionEntry))
                    {
                      case "EQ":
                        break;
                      case "GE":
                        break;
                      case "GT":
                        break;
                      case "LE":
                        break;
                      case "LT":
                        break;
                      case "NE":
                        break;
                      default:
                        export.EabFileHandling.Status = "OPERATOR";

                        goto AfterCycle;
                    }
                  }

                  // ---------------------------------------------------
                  // Remove operator from local_document_line_remain
                  // ---------------------------------------------------
                  local.DocumentLineRemainWidth.Count -= 2;

                  if (local.DocumentLineRemainWidth.Count > 0)
                  {
                    local.DocumentLineRemain.Text80 =
                      Substring(local.DocumentLineRemain.Text80, 3,
                      local.DocumentLineRemainWidth.Count);
                  }
                  else
                  {
                    local.DocumentLineRemain.Text80 = "";
                  }

                  break;
                case '2':
                  if (local.EquationIgnoreLevel.Count == 0)
                  {
                    local.DocumentLineTemp.Text80 =
                      ToUpper(local.DocumentLineRemain.Text80);
                    local.NextPosition.Count =
                      Find(local.DocumentLineTemp.Text80, "\\E");

                    if (local.NextPosition.Count <= 0)
                    {
                      export.EabFileHandling.Status = "OP2LNGTH";

                      goto AfterCycle;
                    }

                    local.EquationOperand2.Text20 =
                      Substring(local.DocumentLineRemain.Text80, 1,
                      local.NextPosition.Count - 1);
                    local.TempCalcCommon.ActionEntry =
                      Substring(local.EquationOperand2.Text20, 1, 2);
                    local.TempCalcCommon.ActionEntry =
                      ToUpper(local.TempCalcCommon.ActionEntry);

                    if (Equal(local.TempCalcCommon.ActionEntry, "\\F"))
                    {
                      local.EquationOperand.Count = 2;
                    }
                    else
                    {
                      // ---------------------------------------------------
                      // Remove operand2 from local_document_line_remain
                      // ---------------------------------------------------
                      local.DocumentLineRemainWidth.Count =
                        local.DocumentLineRemainWidth.Count - local
                        .NextPosition.Count + 1;

                      if (local.DocumentLineRemainWidth.Count > 0)
                      {
                        local.DocumentLineRemain.Text80 =
                          Substring(local.DocumentLineRemain.Text80,
                          local.NextPosition.Count,
                          local.DocumentLineRemainWidth.Count);
                      }
                      else
                      {
                        local.DocumentLineRemain.Text80 = "";
                      }
                    }
                  }

                  break;
                case '3':
                  if (local.EquationIgnoreLevel.Count == 0)
                  {
                    local.EquationResultToSkip.Flag = "";
                    local.DocumentLineTemp.Text80 =
                      ToUpper(local.DocumentLineRemain.Text80);
                    local.NextPosition.Count =
                      Find(local.DocumentLineTemp.Text80, "\\E");

                    if (AsChar(local.EquationNesting.Item.GlocalEquationResult.
                      Flag) == 'T')
                    {
                      local.NoSpace.Flag = "Y";
                    }
                    else
                    {
                      local.EquationResultToSkip.Flag = "T";

                      // mjr
                      // -----------------------------------------------
                      // Since the result is FALSE, this equation can be thrown 
                      // away
                      // --------------------------------------------------
                      if (local.DocumentLineRemainWidth.Count > 0)
                      {
                        local.DocumentLineRemainWidth.Count =
                          local.DocumentLineRemainWidth.Count - local
                          .NextPosition.Count + 1;

                        if (local.DocumentLineRemainWidth.Count > 0)
                        {
                          local.DocumentLineRemain.Text80 =
                            Substring(local.DocumentLineRemain.Text80,
                            local.NextPosition.Count,
                            local.DocumentLineRemainWidth.Count);
                        }
                        else
                        {
                          local.DocumentLineRemain.Text80 = "";
                        }
                      }
                      else
                      {
                      }
                    }
                  }

                  break;
                case '4':
                  if (local.EquationIgnoreLevel.Count == 0)
                  {
                    local.EquationResultToSkip.Flag = "";
                    local.DocumentLineTemp.Text80 =
                      ToUpper(local.DocumentLineRemain.Text80);
                    local.NextPosition.Count =
                      Find(local.DocumentLineTemp.Text80, "\\E");

                    if (AsChar(local.EquationNesting.Item.GlocalEquationResult.
                      Flag) == 'T')
                    {
                      local.EquationResultToSkip.Flag = "F";

                      // mjr
                      // -----------------------------------------------
                      // Since the result is TRUE, this equation can be thrown 
                      // away
                      // --------------------------------------------------
                      if (local.DocumentLineRemainWidth.Count > 0)
                      {
                        local.DocumentLineRemainWidth.Count =
                          local.DocumentLineRemainWidth.Count - local
                          .NextPosition.Count + 1;

                        if (local.DocumentLineRemainWidth.Count > 0)
                        {
                          local.DocumentLineRemain.Text80 =
                            Substring(local.DocumentLineRemain.Text80,
                            local.NextPosition.Count,
                            local.DocumentLineRemainWidth.Count);
                        }
                        else
                        {
                          local.DocumentLineRemain.Text80 = "";
                        }
                      }
                      else
                      {
                      }
                    }
                    else
                    {
                      local.NoSpace.Flag = "Y";
                    }
                  }

                  break;
                case '5':
                  if (local.EquationIgnoreLevel.Count == 0)
                  {
                    local.EquationNesting.Update.GlocalEquationResult.Flag = "";

                    --local.EquationNesting.Index;
                    local.EquationNesting.CheckSize();

                    local.EquationNesting.Count =
                      local.EquationNesting.Index + 1;

                    if (local.EquationNesting.Index == -1)
                    {
                      local.EquationResultToSkip.Flag = "";
                    }
                    else if (AsChar(local.EquationResultToSkip.Flag) != AsChar
                      (local.EquationNesting.Item.GlocalEquationResult.Flag))
                    {
                      local.EquationResultToSkip.Flag = "";
                    }
                  }
                  else
                  {
                    --local.EquationIgnoreLevel.Count;
                  }

                  break;
                default:
                  export.EabFileHandling.Status = "EQUATION";

                  goto AfterCycle;
              }

              break;
            case 'F':
              // -------------------------------------------------
              // Field value marker
              // -------------------------------------------------
              // ---------------------------------------------------
              // Extract \f:field_name from local_document_line_remain
              // ---------------------------------------------------
              local.Field.Name =
                Substring(local.DocumentLineRemain.Text80,
                local.Position.Count + 3, 10);
              local.Field.Name = ToUpper(local.Field.Name);
              local.Position.Count = Find(local.Field.Name, "\\");
              local.NextPosition.Count =
                Find(String(local.Field.Name, Field.Name_MaxLength), " ");

              if (local.Position.Count > 0)
              {
                if (local.NextPosition.Count > 0)
                {
                  if (local.Position.Count > local.NextPosition.Count)
                  {
                    local.Field.Name =
                      Substring(local.Field.Name, 1, local.NextPosition.Count -
                      1);
                  }
                  else
                  {
                    local.Field.Name =
                      Substring(local.Field.Name, 1, local.Position.Count - 1);
                    local.NextPosition.Count = local.Position.Count;
                  }
                }
                else
                {
                  local.Field.Name =
                    Substring(local.Field.Name, 1, local.Position.Count - 1);
                  local.NextPosition.Count = local.Position.Count;
                }
              }
              else if (local.NextPosition.Count > 0)
              {
                local.Field.Name =
                  Substring(local.Field.Name, 1, local.NextPosition.Count - 1);
              }
              else
              {
                local.NextPosition.Count = 11;
              }

              local.Length.Count = local.NextPosition.Count - 1;

              // ---------------------------------------------------
              // Replace "_" with " "
              // ---------------------------------------------------
              do
              {
                local.Position.Count = Find(local.Field.Name, "_");

                if (local.Position.Count > 1)
                {
                  local.Field.Name =
                    Substring(local.Field.Name, Field.Name_MaxLength, 1,
                    local.Position.Count - 1) + " " + Substring
                    (local.Field.Name, Field.Name_MaxLength,
                    local.Position.Count + 1, 10);
                }
                else if (local.Position.Count > 0)
                {
                  local.Field.Name = " " + Substring
                    (local.Field.Name, Field.Name_MaxLength, 2, 10);
                }
                else
                {
                }
              }
              while(local.Position.Count != 0);

              if (ReadFieldValue())
              {
                local.FieldValue.Value = entities.FieldValue.Value;
              }
              else
              {
                // ------------------------------------------------------------
                // Determine whether the user entered a proper field name
                // ------------------------------------------------------------
                if (ReadField())
                {
                  if (ReadDocumentField())
                  {
                    local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
                  }
                  else
                  {
                    export.EabFileHandling.Status = "DOCFLD";

                    goto AfterCycle;
                  }
                }
                else
                {
                  export.EabFileHandling.Status = "FIELDNM";

                  goto AfterCycle;
                }
              }

              // ---------------------------------------------------
              // Remove \f:field_name from local_document_line_remain
              // ---------------------------------------------------
              local.DocumentLineRemainWidth.Count =
                local.DocumentLineRemainWidth.Count - local.Length.Count - 3;
              local.DocumentLineRemain.Text80 =
                Substring(local.DocumentLineRemain.Text80, local.Length.Count +
                4, local.DocumentLineRemainWidth.Count);
              local.DocumentLineTemp.Text80 =
                ToUpper(local.DocumentLineRemain.Text80);

              // ---------------------------------------------------
              // Determine if field_name has a length specification
              // ---------------------------------------------------
              local.FieldLengthSpecification.Count = 0;

              if (Equal(local.DocumentLineTemp.Text80, 1, 3, "\\L:"))
              {
                local.TempCalcCommon.ActionEntry =
                  Substring(local.DocumentLineRemain.Text80, 4, 2);

                if (!Lt(local.TempCalcCommon.ActionEntry, "00") && !
                  Lt("99", local.TempCalcCommon.ActionEntry))
                {
                  local.FieldLengthSpecification.Count =
                    (int)StringToNumber(local.TempCalcCommon.ActionEntry);
                }

                if (local.FieldLengthSpecification.Count > 80)
                {
                  local.FieldLengthSpecification.Count = 80;
                }
                else if (local.FieldLengthSpecification.Count < 0)
                {
                  local.FieldLengthSpecification.Count = 0;
                }

                // ---------------------------------------------------
                // Remove field length specification
                // ---------------------------------------------------
                local.DocumentLineRemainWidth.Count -= 5;
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80, 6,
                  local.DocumentLineRemainWidth.Count);
                local.DocumentLineTemp.Text80 =
                  ToUpper(local.DocumentLineRemain.Text80);

                // ---------------------------------------------------
                // Determine if field_name is to be right justified
                // ---------------------------------------------------
                if (Equal(local.DocumentLineTemp.Text80, 1, 2, "\\R"))
                {
                  local.TempCalcCommon.ActionEntry = "R";

                  // ---------------------------------------------------
                  // Remove justification specification
                  // ---------------------------------------------------
                  local.DocumentLineRemainWidth.Count -= 2;
                  local.DocumentLineRemain.Text80 =
                    Substring(local.DocumentLineRemain.Text80, 3,
                    local.DocumentLineRemainWidth.Count);
                }
                else
                {
                  local.TempCalcCommon.ActionEntry = "L";
                }
              }

              // ---------------------------------------------------
              // Replace "\" with "_"
              // ---------------------------------------------------
              local.Length.Count = Length(TrimEnd(local.FieldValue.Value));

              do
              {
                local.Position.Count = Find(local.FieldValue.Value, "\\");

                if (local.Position.Count > 1)
                {
                  local.FieldValue.Value =
                    Substring(local.FieldValue.Value, 1, local.Position.Count -
                    1) + "_" + Substring
                    (local.FieldValue.Value, local.Position.Count +
                    1, local.Length.Count);
                }
                else if (local.Position.Count > 0)
                {
                  local.FieldValue.Value = "_" + Substring
                    (local.FieldValue.Value, 2, local.Length.Count);
                }
                else
                {
                }
              }
              while(local.Position.Count != 0);

              // ---------------------------------------------------
              // Replace "_" with "\\"
              // ---------------------------------------------------
              do
              {
                local.Length.Count = Length(TrimEnd(local.FieldValue.Value));
                local.Position.Count = Find(local.FieldValue.Value, "_");

                if (local.Position.Count > 1)
                {
                  local.FieldValue.Value =
                    Substring(local.FieldValue.Value, 1, local.Position.Count -
                    1) + "\\\\" + Substring
                    (local.FieldValue.Value, local.Position.Count +
                    1, local.Length.Count);

                  if (local.FieldLengthSpecification.Count > 0)
                  {
                    ++local.FieldLengthSpecification.Count;
                  }
                }
                else if (local.Position.Count > 0)
                {
                  local.FieldValue.Value = "\\\\" + Substring
                    (local.FieldValue.Value, 2, local.Length.Count);

                  if (local.FieldLengthSpecification.Count > 0)
                  {
                    ++local.FieldLengthSpecification.Count;
                  }
                }
                else
                {
                }
              }
              while(local.Position.Count != 0);

              if (local.EquationOperand.Count > 0)
              {
                if (IsEmpty(local.FieldValue.Value))
                {
                  local.FieldValue.Value = "SPACES";
                }

                if (local.EquationOperand.Count == 1)
                {
                  local.EquationOperand1.Text20 = local.FieldValue.Value ?? Spaces
                    (20);
                }
                else
                {
                  local.EquationOperand2.Text20 = local.FieldValue.Value ?? Spaces
                    (20);
                }

                local.EquationOperand.Count = 0;
              }
              else if (IsEmpty(local.FieldValue.Value))
              {
                if (local.FieldLengthSpecification.Count == 0)
                {
                }
                else
                {
                  local.DocumentLineRemainWidth.Count += local.
                    FieldLengthSpecification.Count;
                  local.DocumentLineRemain.Text80 =
                    Substring(local.Null1.Text80, WorkArea.Text80_MaxLength, 1,
                    local.FieldLengthSpecification.Count) + local
                    .DocumentLineRemain.Text80;
                }
              }
              else
              {
                if (local.DocmntLineAfterCcWidth.Count > 0)
                {
                  if (local.DocmntLineOverflowWidth.Count > 0)
                  {
                    local.TempCalcCommon.Count =
                      local.DocumentLineRemainWidth.Count + local
                      .DocmntLineAfterCcWidth.Count + local
                      .DocmntLineOverflowWidth.Count;

                    if (local.TempCalcCommon.Count > 160)
                    {
                      export.EabFileHandling.Status = "LOCSTORG";

                      goto AfterCycle;
                    }
                    else
                    {
                      // mjr
                      // ----------------------------------------------------------
                      // Combine after_cc and overflow and line_remain
                      // into after_cc and overflow
                      // -------------------------------------------------------------
                      if (local.DocumentLineRemainWidth.Count + local
                        .DocmntLineAfterCcWidth.Count > 80)
                      {
                        if (local.DocmntLineAfterCcWidth.Count + local
                          .DocmntLineOverflowWidth.Count > 80)
                        {
                          // mjr
                          // ---------------------------------------------------
                          // After_cc will need to be split.  Be careful to not 
                          // split
                          // a special character.
                          // If no special characters exist, split anywhere.
                          // If a special character exists in the part that is 
                          // to be
                          // moved to overflow, disregard the special character.
                          // Otherwise...
                          // ------------------------------------------------------
                          local.TempCalcCommon.Subscript =
                            Find(local.DocmntLineAfterCc.Text80, "\\");
                          local.TempCalcCommon.Count =
                            local.DocmntLineAfterCcWidth.Count + local
                            .DocmntLineOverflowWidth.Count - 79;

                          if (local.TempCalcCommon.Subscript == 0 || local
                            .TempCalcCommon.Subscript >= local
                            .TempCalcCommon.Count)
                          {
                            local.DocmntLineOverflowWidth.Count = 80;
                            local.DocmntLineOverflow.Text80 =
                              Substring(local.DocmntLineAfterCc.Text80,
                              WorkArea.Text80_MaxLength,
                              local.TempCalcCommon.Count, 80 -
                              local.DocmntLineOverflowWidth.Count) + local
                              .DocmntLineOverflow.Text80;
                            local.DocmntLineAfterCcWidth.Count =
                              local.DocumentLineRemainWidth.Count + local
                              .TempCalcCommon.Count;
                            local.DocmntLineAfterCc.Text80 =
                              Substring(local.DocumentLineRemain.Text80,
                              WorkArea.Text80_MaxLength, 1,
                              local.DocumentLineRemainWidth.Count) + Substring
                              (local.DocmntLineAfterCc.Text80,
                              WorkArea.Text80_MaxLength, 1,
                              local.TempCalcCommon.Count - 1);
                          }
                          else
                          {
                            export.EabFileHandling.Status = "LOCSTORG";

                            goto AfterCycle;
                          }
                        }
                        else
                        {
                          local.DocmntLineOverflowWidth.Count =
                            local.DocmntLineAfterCcWidth.Count + local
                            .DocmntLineOverflowWidth.Count;
                          local.DocmntLineOverflow.Text80 =
                            Substring(local.DocmntLineAfterCc.Text80,
                            WorkArea.Text80_MaxLength, 1,
                            local.DocmntLineAfterCcWidth.Count) + local
                            .DocmntLineOverflow.Text80;
                          local.DocmntLineAfterCcWidth.Count =
                            local.DocumentLineRemainWidth.Count;
                          local.DocmntLineAfterCc.Text80 =
                            local.DocumentLineRemain.Text80;
                        }
                      }
                      else
                      {
                        local.DocmntLineAfterCcWidth.Count =
                          local.DocumentLineRemainWidth.Count + local
                          .DocmntLineAfterCcWidth.Count;
                        local.DocmntLineAfterCc.Text80 =
                          Substring(local.DocumentLineRemain.Text80,
                          WorkArea.Text80_MaxLength, 1,
                          local.DocumentLineRemainWidth.Count) + local
                          .DocmntLineAfterCc.Text80;
                      }
                    }
                  }
                  else
                  {
                    local.DocmntLineOverflowWidth.Count =
                      local.DocmntLineAfterCcWidth.Count;
                    local.DocmntLineOverflow.Text80 =
                      local.DocmntLineAfterCc.Text80;
                    local.DocmntLineAfterCcWidth.Count =
                      local.DocumentLineRemainWidth.Count;
                    local.DocmntLineAfterCc.Text80 =
                      local.DocumentLineRemain.Text80;
                  }
                }
                else
                {
                  local.DocmntLineAfterCcWidth.Count =
                    local.DocumentLineRemainWidth.Count;
                  local.DocmntLineAfterCc.Text80 =
                    local.DocumentLineRemain.Text80;
                }

                if (local.FieldLengthSpecification.Count == 0)
                {
                  local.DocumentLineRemain.Text80 = local.FieldValue.Value ?? Spaces
                    (80);
                  local.DocumentLineRemainWidth.Count =
                    Length(TrimEnd(local.DocumentLineRemain.Text80));
                }
                else
                {
                  local.Length.Count = Length(TrimEnd(local.FieldValue.Value));

                  if (local.Length.Count == local
                    .FieldLengthSpecification.Count)
                  {
                    local.DocumentLineRemainWidth.Count =
                      local.FieldLengthSpecification.Count;
                    local.DocumentLineRemain.Text80 =
                      local.FieldValue.Value ?? Spaces(80);
                  }
                  else if (local.Length.Count > local
                    .FieldLengthSpecification.Count)
                  {
                    local.DocumentLineRemainWidth.Count =
                      local.FieldLengthSpecification.Count;
                    local.DocumentLineRemain.Text80 =
                      Substring(local.FieldValue.Value, 1,
                      local.FieldLengthSpecification.Count);
                  }
                  else
                  {
                    // ---------------------------------------------------
                    // Determine if field_name is to be right justified
                    // ---------------------------------------------------
                    if (Equal(local.TempCalcCommon.ActionEntry, "R"))
                    {
                      local.DocumentLineRemainWidth.Count =
                        local.FieldLengthSpecification.Count;
                      local.DocumentLineRemain.Text80 =
                        Substring(local.Null1.Text80, WorkArea.Text80_MaxLength,
                        1, local.FieldLengthSpecification.Count -
                        local.Length.Count) + (local.FieldValue.Value ?? "");
                    }
                    else
                    {
                      local.DocumentLineRemainWidth.Count =
                        local.FieldLengthSpecification.Count;
                      local.DocumentLineRemain.Text80 =
                        local.FieldValue.Value ?? Spaces(80);
                    }
                  }
                }
              }

              local.TempCalcCommon.ActionEntry = "";
              local.NoSpace.Flag = "Y";

              break;
            case 'I':
              // -------------------------------------------------
              // Indent Paragraph Marker
              // -------------------------------------------------
              // ---------------------------------------------------
              // Extract \i:position
              // ---------------------------------------------------
              local.TempCalcCommon.ActionEntry =
                Substring(local.DocumentLineRemain.Text80,
                local.Position.Count + 3, 2);

              if (!Lt(local.TempCalcCommon.ActionEntry, "00") && !
                Lt("99", local.TempCalcCommon.ActionEntry))
              {
                local.IndentParagraph.Count =
                  (int)(StringToNumber(local.TempCalcCommon.ActionEntry) - 1);
              }
              else
              {
                local.IndentParagraph.Count = 0;
              }

              if (local.IndentParagraph.Count > local.RptDtlMaxWidth.Count)
              {
                local.IndentParagraph.Count = local.RptDtlMaxWidth.Count;
              }
              else if (local.IndentParagraph.Count < 0)
              {
                local.IndentParagraph.Count = 0;
              }

              // ---------------------------------------------------
              // Remove \i:position from local_document_line_remain
              // ---------------------------------------------------
              local.DocumentLineRemainWidth.Count -= 5;

              if (local.DocumentLineRemainWidth.Count > 0)
              {
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80, 6,
                  local.DocumentLineRemainWidth.Count);
              }
              else
              {
                local.DocumentLineRemain.Text80 = "";
              }

              local.DocumentLineTemp.Text80 =
                ToUpper(local.DocumentLineRemain.Text80);

              // ---------------------------------------------------
              // Determine if first line of paragraph is to be indented
              // ---------------------------------------------------
              if (Equal(local.DocumentLineTemp.Text80, 1, 2, "\\F"))
              {
                local.TempCalcCommon.ActionEntry = "F";

                // ---------------------------------------------------
                // Remove first line indent specification
                // ---------------------------------------------------
                local.DocumentLineRemainWidth.Count -= 2;
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80, 3,
                  local.DocumentLineRemainWidth.Count);
              }
              else
              {
                local.TempCalcCommon.ActionEntry = "";
              }

              if (Equal(local.TempCalcCommon.ActionEntry, "F"))
              {
                if (local.RptDtlWidth.Count < local.IndentParagraph.Count)
                {
                  local.DocumentLineRemainWidth.Count += 5;
                  local.DocumentLineRemain.Text80 = "\\t:" + NumberToString
                    ((long)local.IndentParagraph.Count + 1, 14, 2) + local
                    .DocumentLineRemain.Text80;

                  if (local.PageLinesCount.Count < local
                    .MailingMachLinesIndent.Count)
                  {
                    local.IndentParagraph.Count -= local.
                      MailingMachineMarkWidth.Count;
                  }
                }
                else if (local.PageLinesCount.Count < local
                  .MailingMachLinesIndent.Count)
                {
                  local.IndentParagraph.Count = local.RptDtlWidth.Count - local
                    .MailingMachineMarkWidth.Count + 1;
                }
                else
                {
                  local.IndentParagraph.Count = local.RptDtlWidth.Count + 1;
                }
              }

              local.TempCalcCommon.ActionEntry = "";

              break;
            case 'N':
              // -------------------------------------------------
              // New Line Marker
              // -------------------------------------------------
              // ---------------------------------------------------
              // Remove \n from local_document_line_remain
              // ---------------------------------------------------
              local.DocumentLineRemainWidth.Count -= 2;

              if (local.DocumentLineRemainWidth.Count > 0)
              {
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80, 3,
                  local.DocumentLineRemainWidth.Count);
              }
              else
              {
                local.DocumentLineRemain.Text80 = "";
              }

              // mjr
              // -----------------------------------------------------
              // End paragraph indenting, as the paragraph has ended
              // --------------------------------------------------------
              local.IndentParagraph.Count = 0;

              // ---------------------------------------------------
              // Write line
              // ---------------------------------------------------
              if (local.RptDtlWidth.Count > 0)
              {
                if (AsChar(local.CenterRptDtl.Flag) == 'Y')
                {
                  local.TempCalcCommon.Count =
                    (80 - local.RptDtlWidth.Count) / 2;
                  local.CenterRptDtl.Flag = "N";
                  local.EabReportSend.RptDetail =
                    Substring(local.Null1.Text40, WorkArea.Text40_MaxLength, 1,
                    local.TempCalcCommon.Count) + local
                    .EabReportSend.RptDetail;
                }
                else
                {
                  local.EabReportSend.RptDetail =
                    Substring(local.Null1.Text20, WorkArea.Text20_MaxLength, 1,
                    local.DocumentTemplates.Item.GlocalMarginWidth.Count) + local
                    .EabReportSend.RptDetail;
                }
              }
              else
              {
              }

              if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
              {
                export.EabFileHandling.Status = "PRTOVRFL";

                goto AfterCycle;
              }

              ++local.Print.Index;
              local.Print.CheckSize();

              MoveEabReportSend(local.EabReportSend,
                local.Print.Update.GlocalRptDtlLine);
              local.EabReportSend.RptDetail = "";
              local.RptDtlWidth.Count = 0;

              if (local.PageLinesCount.Count >= local
                .DocumentTemplates.Item.GlocalPageMaxLines.Count)
              {
                local.EabReportSend.Command = "NEWPAGE";
                local.PageLinesCount.Count = 0;
              }
              else
              {
                local.EabReportSend.Command = "DETAIL";
                ++local.PageLinesCount.Count;
              }

              if (local.PageLinesCount.Count < local
                .MailingMachLinesIndent.Count)
              {
                local.RptDtlWidth.Count = local.MailingMachineMarkWidth.Count;
              }

              break;
            case 'P':
              // -------------------------------------------------
              // New Page Marker
              // -------------------------------------------------
              if (!IsEmpty(local.EabReportSend.RptDetail))
              {
                if (AsChar(local.CenterRptDtl.Flag) == 'Y')
                {
                  local.TempCalcCommon.Count =
                    (80 - local.RptDtlWidth.Count) / 2;
                  local.CenterRptDtl.Flag = "N";
                  local.EabReportSend.RptDetail =
                    Substring(local.Null1.Text40, WorkArea.Text40_MaxLength, 1,
                    local.TempCalcCommon.Count) + local
                    .EabReportSend.RptDetail;
                }
                else
                {
                  local.EabReportSend.RptDetail =
                    Substring(local.Null1.Text20, WorkArea.Text20_MaxLength, 1,
                    local.DocumentTemplates.Item.GlocalMarginWidth.Count) + local
                    .EabReportSend.RptDetail;
                }
              }

              if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
              {
                export.EabFileHandling.Status = "PRTOVRFL";

                goto AfterCycle;
              }

              ++local.Print.Index;
              local.Print.CheckSize();

              MoveEabReportSend(local.EabReportSend,
                local.Print.Update.GlocalRptDtlLine);
              local.EabReportSend.RptDetail = "";
              local.RptDtlWidth.Count = local.MailingMachineMarkWidth.Count;
              local.EabReportSend.Command = "NEWPAGE";
              local.PageLinesCount.Count = 0;

              // ---------------------------------------------------
              // Remove \p from local_document_line_remain
              // ---------------------------------------------------
              local.DocumentLineRemainWidth.Count -= 2;

              if (local.DocumentLineRemainWidth.Count > 0)
              {
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80, 3,
                  local.DocumentLineRemainWidth.Count);
              }
              else
              {
                local.DocumentLineRemain.Text80 = "";
              }

              // mjr
              // -----------------------------------------------------
              // End paragraph indenting, as the paragraph has ended
              // --------------------------------------------------------
              local.IndentParagraph.Count = 0;

              break;
            case 'T':
              // -------------------------------------------------
              // Tab Marker (Positioning)
              // -------------------------------------------------
              // ---------------------------------------------------
              // Extract \t:position
              // ---------------------------------------------------
              local.TempCalcCommon.ActionEntry =
                Substring(local.DocumentLineRemain.Text80,
                local.Position.Count + 3, 2);

              if (!Lt(local.TempCalcCommon.ActionEntry, "00") && !
                Lt("99", local.TempCalcCommon.ActionEntry))
              {
                local.NextPosition.Count =
                  (int)StringToNumber(local.TempCalcCommon.ActionEntry);
              }
              else
              {
                local.NextPosition.Count = 0;
              }

              if (local.NextPosition.Count > local.RptDtlMaxWidth.Count)
              {
                local.NextPosition.Count = local.RptDtlMaxWidth.Count;
              }
              else if (local.NextPosition.Count < 0)
              {
                local.NextPosition.Count = 0;
              }

              // ---------------------------------------------------
              // Remove \t:position from local_document_line_remain
              // ---------------------------------------------------
              local.DocumentLineRemainWidth.Count -= 5;

              if (local.DocumentLineRemainWidth.Count > 0)
              {
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80, 6,
                  local.DocumentLineRemainWidth.Count);
              }
              else
              {
                local.DocumentLineRemain.Text80 = "";
              }

              if (local.RptDtlWidth.Count < local.NextPosition.Count)
              {
                if (local.DocumentLineRemainWidth.Count + local
                  .NextPosition.Count - local.RptDtlWidth.Count - 1 > local
                  .RptDtlMaxWidth.Count)
                {
                  local.Length.Count = local.NextPosition.Count;

                  for(local.NextPosition.Count = local.RptDtlMaxWidth.Count - local
                    .RptDtlWidth.Count - local.Length.Count; local
                    .NextPosition.Count >= 1; local.NextPosition.Count += -1)
                  {
                    if (IsEmpty(Substring(
                      local.DocumentLineRemain.Text80, local.NextPosition.Count,
                      1)))
                    {
                      break;
                    }
                  }

                  // mjr
                  // -----------------------------------------------
                  // 03/27/2001
                  // Remaining line can be split in the available range
                  // ------------------------------------------------------------
                  if (local.NextPosition.Count > 0)
                  {
                    if (local.DocmntLineAfterCcWidth.Count > 0)
                    {
                      if (local.DocmntLineOverflowWidth.Count > 0)
                      {
                        local.TempCalcCommon.Count =
                          local.DocumentLineRemainWidth.Count + local
                          .DocmntLineAfterCcWidth.Count + local
                          .DocmntLineOverflowWidth.Count - local
                          .NextPosition.Count;

                        if (local.TempCalcCommon.Count > 160)
                        {
                          export.EabFileHandling.Status = "LOCSTORG";

                          return;
                        }
                        else
                        {
                          // mjr
                          // ----------------------------------------------------------
                          // Combine after_cc and overflow and the new part
                          // into after_cc and overflow
                          // -------------------------------------------------------------
                          if (local.DocumentLineRemainWidth.Count + local
                            .DocmntLineAfterCcWidth.Count - local
                            .NextPosition.Count > 80)
                          {
                            if (local.DocmntLineAfterCcWidth.Count + local
                              .DocmntLineOverflowWidth.Count > 80)
                            {
                              // mjr
                              // ---------------------------------------------------
                              // After_cc will need to be split.  Be careful to 
                              // not split
                              // a special character.
                              // If no special characters exist, split anywhere.
                              // If a special character exists in the part that 
                              // is to be
                              // moved to overflow, disregard the special 
                              // character.
                              // Otherwise...
                              // ------------------------------------------------------
                              local.TempCalcCommon.Subscript =
                                Find(local.DocmntLineAfterCc.Text80, "\\");
                              local.TempCalcCommon.Count =
                                local.DocmntLineAfterCcWidth.Count + local
                                .DocmntLineOverflowWidth.Count - 79;

                              if (local.TempCalcCommon.Subscript == 0 || local
                                .TempCalcCommon.Subscript >= local
                                .TempCalcCommon.Count)
                              {
                                local.DocmntLineOverflowWidth.Count = 80;
                                local.DocmntLineOverflow.Text80 =
                                  Substring(local.DocmntLineAfterCc.Text80,
                                  WorkArea.Text80_MaxLength,
                                  local.TempCalcCommon.Count, 80 -
                                  local.DocmntLineOverflowWidth.Count) + local
                                  .DocmntLineOverflow.Text80;
                                local.DocmntLineAfterCcWidth.Count =
                                  local.DocumentLineRemainWidth.Count + local
                                  .TempCalcCommon.Count - local
                                  .NextPosition.Count;
                                local.DocmntLineAfterCc.Text80 =
                                  Substring(local.DocumentLineRemain.Text80,
                                  WorkArea.Text80_MaxLength,
                                  local.NextPosition.Count +
                                  1, local.DocumentLineRemainWidth.Count -
                                  local.NextPosition.Count) + Substring
                                  (local.DocmntLineAfterCc.Text80,
                                  WorkArea.Text80_MaxLength, 1,
                                  local.TempCalcCommon.Count - 1);
                              }
                              else
                              {
                                export.EabFileHandling.Status = "LOCSTORG";

                                return;
                              }
                            }
                            else
                            {
                              local.DocmntLineOverflowWidth.Count =
                                local.DocmntLineAfterCcWidth.Count + local
                                .DocmntLineOverflowWidth.Count;
                              local.DocmntLineOverflow.Text80 =
                                Substring(local.DocmntLineAfterCc.Text80,
                                WorkArea.Text80_MaxLength, 1,
                                local.DocmntLineAfterCcWidth.Count) + local
                                .DocmntLineOverflow.Text80;
                              local.DocmntLineAfterCcWidth.Count =
                                local.DocumentLineRemainWidth.Count - local
                                .NextPosition.Count;
                              local.DocmntLineAfterCc.Text80 =
                                Substring(local.DocumentLineRemain.Text80,
                                local.NextPosition.Count +
                                1, local.DocmntLineAfterCcWidth.Count);
                            }
                          }
                          else
                          {
                            local.DocmntLineAfterCcWidth.Count =
                              local.DocumentLineRemainWidth.Count + local
                              .DocmntLineAfterCcWidth.Count - local
                              .NextPosition.Count;
                            local.DocmntLineAfterCc.Text80 =
                              Substring(local.DocumentLineRemain.Text80,
                              WorkArea.Text80_MaxLength,
                              local.NextPosition.Count +
                              1, local.DocumentLineRemainWidth.Count -
                              local.NextPosition.Count) + local
                              .DocmntLineAfterCc.Text80;
                          }
                        }
                      }
                      else
                      {
                        local.DocmntLineOverflowWidth.Count =
                          local.DocmntLineAfterCcWidth.Count;
                        local.DocmntLineOverflow.Text80 =
                          local.DocmntLineAfterCc.Text80;
                        local.DocmntLineAfterCcWidth.Count =
                          local.DocumentLineRemainWidth.Count - local
                          .NextPosition.Count;
                        local.DocmntLineAfterCc.Text80 =
                          Substring(local.DocumentLineRemain.Text80,
                          local.NextPosition.Count +
                          1, local.DocmntLineAfterCcWidth.Count);
                      }
                    }
                    else
                    {
                      local.DocmntLineAfterCcWidth.Count =
                        local.DocumentLineRemainWidth.Count - local
                        .NextPosition.Count;
                      local.DocmntLineAfterCc.Text80 =
                        Substring(local.DocumentLineRemain.Text80,
                        local.NextPosition.Count +
                        1, local.DocmntLineAfterCcWidth.Count);
                    }

                    local.DocumentLineRemainWidth.Count =
                      local.DocumentLineRemainWidth.Count - local
                      .DocmntLineAfterCcWidth.Count + local.Length.Count - local
                      .RptDtlWidth.Count - 1;
                    local.DocumentLineRemain.Text80 =
                      Substring(local.Null1.Text80, WorkArea.Text80_MaxLength,
                      1, local.Length.Count - local.RptDtlWidth.Count - 1) + Substring
                      (local.DocumentLineRemain.Text80,
                      WorkArea.Text80_MaxLength, 1, local.NextPosition.Count -
                      1);
                  }
                  else
                  {
                    // mjr
                    // -----------------------------------------------
                    // 03/27/2001
                    // No spaces in Remaining line in the available range
                    // Attempt to put it in after_cc and overflow
                    // ------------------------------------------------------------
                    local.NextPosition.Count =
                      Find(String(
                        local.DocumentLineRemain.Text80,
                      WorkArea.Text80_MaxLength), " ");

                    if ((local.NextPosition.Count <= 0 || local
                      .NextPosition.Count > local
                      .DocumentLineRemainWidth.Count) && local
                      .DocumentLineRemainWidth.Count > local
                      .RptDtlMaxWidth.Count)
                    {
                      // mjr
                      // -----------------------------------------------
                      // 02/23/2000
                      // Line cannot be split since there are no spaces nor 
                      // hypens
                      // Add a hypen at the end of the line and split it there.
                      // ------------------------------------------------------------
                      local.NextPosition.Count = local.RptDtlMaxWidth.Count - local
                        .RptDtlWidth.Count - local.Length.Count;

                      if (local.DocmntLineAfterCcWidth.Count > 0)
                      {
                        if (local.DocmntLineOverflowWidth.Count > 0)
                        {
                          local.TempCalcCommon.Count =
                            local.DocumentLineRemainWidth.Count + local
                            .DocmntLineAfterCcWidth.Count + local
                            .DocmntLineOverflowWidth.Count - local
                            .NextPosition.Count;

                          if (local.TempCalcCommon.Count > 160)
                          {
                            export.EabFileHandling.Status = "LOCSTORG";

                            return;
                          }
                          else
                          {
                            // mjr
                            // ----------------------------------------------------------
                            // Combine after_cc and overflow and the new part
                            // into after_cc and overflow
                            // -------------------------------------------------------------
                            if (local.DocumentLineRemainWidth.Count + local
                              .DocmntLineAfterCcWidth.Count - local
                              .NextPosition.Count > 80)
                            {
                              if (local.DocmntLineAfterCcWidth.Count + local
                                .DocmntLineOverflowWidth.Count > 80)
                              {
                                // mjr
                                // ---------------------------------------------------
                                // After_cc will need to be split.  Be careful 
                                // to not split
                                // a special character.
                                // If no special characters exist, split 
                                // anywhere.
                                // If a special character exists in the part 
                                // that is to be
                                // moved to overflow, disregard the special 
                                // character.
                                // Otherwise...
                                // ------------------------------------------------------
                                local.TempCalcCommon.Subscript =
                                  Find(local.DocmntLineAfterCc.Text80, "\\");
                                local.TempCalcCommon.Count =
                                  local.DocmntLineAfterCcWidth.Count + local
                                  .DocmntLineOverflowWidth.Count - 79;

                                if (local.TempCalcCommon.Subscript == 0 || local
                                  .TempCalcCommon.Subscript >= local
                                  .TempCalcCommon.Count)
                                {
                                  local.DocmntLineOverflowWidth.Count = 80;
                                  local.DocmntLineOverflow.Text80 =
                                    Substring(local.DocmntLineAfterCc.Text80,
                                    WorkArea.Text80_MaxLength,
                                    local.TempCalcCommon.Count, 80 -
                                    local.DocmntLineOverflowWidth.Count) + local
                                    .DocmntLineOverflow.Text80;
                                  local.DocmntLineAfterCcWidth.Count =
                                    local.DocumentLineRemainWidth.Count + local
                                    .TempCalcCommon.Count - local
                                    .NextPosition.Count;
                                  local.DocmntLineAfterCc.Text80 =
                                    Substring(local.DocumentLineRemain.Text80,
                                    WorkArea.Text80_MaxLength,
                                    local.NextPosition.Count +
                                    1, local.DocumentLineRemainWidth.Count -
                                    local.NextPosition.Count) + Substring
                                    (local.DocmntLineAfterCc.Text80,
                                    WorkArea.Text80_MaxLength, 1,
                                    local.TempCalcCommon.Count - 1);
                                }
                                else
                                {
                                  export.EabFileHandling.Status = "LOCSTORG";

                                  return;
                                }
                              }
                              else
                              {
                                local.DocmntLineOverflowWidth.Count =
                                  local.DocmntLineAfterCcWidth.Count + local
                                  .DocmntLineOverflowWidth.Count;
                                local.DocmntLineOverflow.Text80 =
                                  Substring(local.DocmntLineAfterCc.Text80,
                                  WorkArea.Text80_MaxLength, 1,
                                  local.DocmntLineAfterCcWidth.Count) + local
                                  .DocmntLineOverflow.Text80;
                                local.DocmntLineAfterCcWidth.Count =
                                  local.DocumentLineRemainWidth.Count - local
                                  .NextPosition.Count;
                                local.DocmntLineAfterCc.Text80 =
                                  Substring(local.DocumentLineRemain.Text80,
                                  local.NextPosition.Count +
                                  1, local.DocmntLineAfterCcWidth.Count);
                              }
                            }
                            else
                            {
                              local.DocmntLineAfterCcWidth.Count =
                                local.DocumentLineRemainWidth.Count + local
                                .DocmntLineAfterCcWidth.Count - local
                                .NextPosition.Count;
                              local.DocmntLineAfterCc.Text80 =
                                Substring(local.DocumentLineRemain.Text80,
                                WorkArea.Text80_MaxLength,
                                local.NextPosition.Count +
                                1, local.DocumentLineRemainWidth.Count -
                                local.NextPosition.Count) + local
                                .DocmntLineAfterCc.Text80;
                            }
                          }
                        }
                        else
                        {
                          local.DocmntLineOverflowWidth.Count =
                            local.DocmntLineAfterCcWidth.Count;
                          local.DocmntLineOverflow.Text80 =
                            local.DocmntLineAfterCc.Text80;
                          local.DocmntLineAfterCcWidth.Count =
                            local.DocumentLineRemainWidth.Count - local
                            .NextPosition.Count;
                          local.DocmntLineAfterCc.Text80 =
                            Substring(local.DocumentLineRemain.Text80,
                            local.NextPosition.Count +
                            1, local.DocmntLineAfterCcWidth.Count);
                        }
                      }
                      else
                      {
                        local.DocmntLineAfterCcWidth.Count =
                          local.DocumentLineRemainWidth.Count - local
                          .NextPosition.Count;
                        local.DocmntLineAfterCc.Text80 =
                          Substring(local.DocumentLineRemain.Text80,
                          local.NextPosition.Count +
                          1, local.DocmntLineAfterCcWidth.Count);
                      }

                      local.DocumentLineRemainWidth.Count =
                        local.RptDtlMaxWidth.Count;
                      local.DocumentLineRemain.Text80 =
                        Substring(local.Null1.Text80, WorkArea.Text80_MaxLength,
                        1, local.Length.Count - 1) + Substring
                        (local.DocumentLineRemain.Text80,
                        WorkArea.Text80_MaxLength, 1,
                        local.NextPosition.Count) + "-";
                    }
                    else
                    {
                      // mjr
                      // -----------------------------------------------
                      // 03/27/2001
                      // The first word in Remaining line is too long to be
                      // added to the current line.
                      // Put the entire Remaining line in after_cc and overflow
                      // ------------------------------------------------------------
                      if (local.DocmntLineAfterCcWidth.Count > 0)
                      {
                        if (local.DocmntLineOverflowWidth.Count > 0)
                        {
                          local.TempCalcCommon.Count =
                            local.DocumentLineRemainWidth.Count + local
                            .DocmntLineAfterCcWidth.Count + local
                            .DocmntLineOverflowWidth.Count;

                          if (local.TempCalcCommon.Count > 160)
                          {
                            export.EabFileHandling.Status = "LOCSTORG";

                            return;
                          }
                          else
                          {
                            // mjr
                            // ----------------------------------------------------------
                            // Combine after_cc and overflow and the new part
                            // into after_cc and overflow
                            // -------------------------------------------------------------
                            if (local.DocumentLineRemainWidth.Count + local
                              .DocmntLineAfterCcWidth.Count > 80)
                            {
                              if (local.DocmntLineAfterCcWidth.Count + local
                                .DocmntLineOverflowWidth.Count > 80)
                              {
                                // mjr
                                // ---------------------------------------------------
                                // After_cc will need to be split.  Be careful 
                                // to not split
                                // a special character.
                                // If no special characters exist, split 
                                // anywhere.
                                // If a special character exists in the part 
                                // that is to be
                                // moved to overflow, disregard the special 
                                // character.
                                // Otherwise...
                                // ------------------------------------------------------
                                local.TempCalcCommon.Subscript =
                                  Find(local.DocmntLineAfterCc.Text80, "\\");
                                local.TempCalcCommon.Count =
                                  local.DocmntLineAfterCcWidth.Count + local
                                  .DocmntLineOverflowWidth.Count - 79;

                                if (local.TempCalcCommon.Subscript == 0 || local
                                  .TempCalcCommon.Subscript >= local
                                  .TempCalcCommon.Count)
                                {
                                  local.DocmntLineOverflowWidth.Count = 80;
                                  local.DocmntLineOverflow.Text80 =
                                    Substring(local.DocmntLineAfterCc.Text80,
                                    WorkArea.Text80_MaxLength,
                                    local.TempCalcCommon.Count, 80 -
                                    local.DocmntLineOverflowWidth.Count) + local
                                    .DocmntLineOverflow.Text80;
                                  local.DocmntLineAfterCcWidth.Count =
                                    local.DocumentLineRemainWidth.Count + local
                                    .TempCalcCommon.Count;
                                  local.DocmntLineAfterCc.Text80 =
                                    Substring(local.DocumentLineRemain.Text80,
                                    WorkArea.Text80_MaxLength, 1,
                                    local.DocumentLineRemainWidth.Count) + Substring
                                    (local.DocmntLineAfterCc.Text80,
                                    WorkArea.Text80_MaxLength, 1,
                                    local.TempCalcCommon.Count - 1);
                                }
                                else
                                {
                                  export.EabFileHandling.Status = "LOCSTORG";

                                  return;
                                }
                              }
                              else
                              {
                                local.DocmntLineOverflowWidth.Count =
                                  local.DocmntLineAfterCcWidth.Count + local
                                  .DocmntLineOverflowWidth.Count;
                                local.DocmntLineOverflow.Text80 =
                                  Substring(local.DocmntLineAfterCc.Text80,
                                  WorkArea.Text80_MaxLength, 1,
                                  local.DocmntLineAfterCcWidth.Count) + local
                                  .DocmntLineOverflow.Text80;
                                local.DocmntLineAfterCcWidth.Count =
                                  local.DocumentLineRemainWidth.Count;
                                local.DocmntLineAfterCc.Text80 =
                                  local.DocumentLineRemain.Text80;
                              }
                            }
                            else
                            {
                              local.DocmntLineAfterCcWidth.Count =
                                local.DocumentLineRemainWidth.Count + local
                                .DocmntLineAfterCcWidth.Count;
                              local.DocmntLineAfterCc.Text80 =
                                Substring(local.DocumentLineRemain.Text80,
                                WorkArea.Text80_MaxLength, 1,
                                local.DocumentLineRemainWidth.Count) + local
                                .DocmntLineAfterCc.Text80;
                            }
                          }
                        }
                        else
                        {
                          local.DocmntLineOverflowWidth.Count =
                            local.DocmntLineAfterCcWidth.Count;
                          local.DocmntLineOverflow.Text80 =
                            local.DocmntLineAfterCc.Text80;
                          local.DocmntLineAfterCcWidth.Count =
                            local.DocumentLineRemainWidth.Count;
                          local.DocmntLineAfterCc.Text80 =
                            local.DocumentLineRemain.Text80;
                        }
                      }
                      else
                      {
                        local.DocmntLineAfterCcWidth.Count =
                          local.DocumentLineRemainWidth.Count;
                        local.DocmntLineAfterCc.Text80 =
                          local.DocumentLineRemain.Text80;
                      }

                      local.DocumentLineRemainWidth.Count =
                        local.Length.Count - local.RptDtlWidth.Count - 1;
                      local.DocumentLineRemain.Text80 = "";
                    }
                  }
                }
                else
                {
                  local.DocumentLineRemainWidth.Count =
                    local.DocumentLineRemainWidth.Count + local
                    .NextPosition.Count - local.RptDtlWidth.Count - 1;
                  local.DocumentLineRemain.Text80 =
                    Substring(local.Null1.Text80, WorkArea.Text80_MaxLength, 1,
                    local.NextPosition.Count - local.RptDtlWidth.Count - 1) + local
                    .DocumentLineRemain.Text80;
                }
              }

              break;
            case '\\':
              // -------------------------------------------------
              // Backslash Marker
              // -------------------------------------------------
              local.EabReportSend.RptDetail =
                Substring(local.EabReportSend.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, local.RptDtlWidth.Count) +
                "\\";
              ++local.RptDtlWidth.Count;

              // ---------------------------------------------------
              // Remove \\ from local_document_line_remain
              // ---------------------------------------------------
              local.DocumentLineRemainWidth.Count -= 2;

              if (local.DocumentLineRemainWidth.Count > 0)
              {
                local.DocumentLineRemain.Text80 =
                  Substring(local.DocumentLineRemain.Text80, 3,
                  local.DocumentLineRemainWidth.Count);
              }
              else
              {
                local.DocumentLineRemain.Text80 = "";
              }

              local.NoSpace.Flag = "Y";

              break;
            default:
              export.EabFileHandling.Status = "SPECIAL";

              goto AfterCycle;
          }
        }

Test2:

        if (local.DocmntLineAfterCcWidth.Count == 0 && local
          .DocmntLineOverflowWidth.Count > 0)
        {
          local.DocmntLineAfterCcWidth.Count =
            local.DocmntLineOverflowWidth.Count;
          local.DocmntLineAfterCc.Text80 = local.DocmntLineOverflow.Text80;
          local.DocmntLineOverflow.Text80 = "";
          local.DocmntLineOverflowWidth.Count = 0;
        }

        if (local.DocumentLineRemainWidth.Count == 0 && local
          .DocmntLineAfterCcWidth.Count > 0)
        {
          local.DocumentLineRemainWidth.Count =
            local.DocmntLineAfterCcWidth.Count;
          local.DocumentLineRemain.Text80 = local.DocmntLineAfterCc.Text80;

          if (local.DocmntLineOverflowWidth.Count > 0)
          {
            local.DocmntLineAfterCcWidth.Count =
              local.DocmntLineOverflowWidth.Count;
            local.DocmntLineAfterCc.Text80 = local.DocmntLineOverflow.Text80;
            local.DocmntLineOverflow.Text80 = "";
            local.DocmntLineOverflowWidth.Count = 0;
          }
          else
          {
            local.DocmntLineAfterCc.Text80 = "";
            local.DocmntLineAfterCcWidth.Count = 0;
          }
        }
      }
      while(local.DocumentLineRemainWidth.Count != 0);
    }

AfterCycle:

    local.DocumentTemplates.Item.SubDocumentLines.CheckIndex();

    // mjr
    // -----------------------------------------------------
    // 03/27/2001
    // WR# 187 Seg H - last line not printed in certain situations
    // Added check for anything not printed
    // ------------------------------------------------------------------
    if (local.RptDtlWidth.Count > 0)
    {
      local.EabReportSend.RptDetail =
        Substring(local.Null1.Text20, WorkArea.Text20_MaxLength, 1,
        local.DocumentTemplates.Item.GlocalMarginWidth.Count) + local
        .EabReportSend.RptDetail;

      if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
      {
        export.EabFileHandling.Status = "PRTOVRFL";

        goto Test3;
      }

      ++local.Print.Index;
      local.Print.CheckSize();

      MoveEabReportSend(local.EabReportSend, local.Print.Update.GlocalRptDtlLine);
        
      local.EabReportSend.RptDetail = "";
      local.RptDtlWidth.Count = 0;

      if (local.PageLinesCount.Count >= local
        .DocumentTemplates.Item.GlocalPageMaxLines.Count)
      {
        local.EabReportSend.Command = "NEWPAGE";
        local.PageLinesCount.Count = 0;
      }
      else
      {
        local.EabReportSend.Command = "DETAIL";
        ++local.PageLinesCount.Count;
      }

      if (local.PageLinesCount.Count < local.MailingMachLinesIndent.Count)
      {
        local.RptDtlWidth.Count = local.MailingMachineMarkWidth.Count;
      }

      // mjr
      // -----------------------------------------------------
      // 04/19/2000
      // If we are indenting and it was the first 9 lines of a page,
      // we subtracted out the mailing mark width.  Add it back
      // in now that we are past the first 9 lines.
      // ------------------------------------------------------------------
      if (local.PageLinesCount.Count == local.MailingMachLinesIndent.Count && local
        .IndentParagraph.Count > 0)
      {
        local.IndentParagraph.Count += local.MailingMachineMarkWidth.Count;
      }

      if (local.IndentParagraph.Count > 0)
      {
        local.RptDtlWidth.Count += local.IndentParagraph.Count;
      }
    }

Test3:

    // -----------------------------------------------------
    // If an error occurred print out the last line
    // (that actually has the error)
    // -----------------------------------------------------
    if (!Equal(export.EabFileHandling.Status, "OK"))
    {
      if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
      {
        export.EabFileHandling.Status = "PRTOVRFL";

        goto Test4;
      }

      ++local.Print.Index;
      local.Print.CheckSize();

      local.Print.Update.GlocalRptDtlLine.RptDetail =
        "+---+----1----+----2----+----3----+----4----+----5----+----6----+----7----+----8";
        

      if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
      {
        export.EabFileHandling.Status = "PRTOVRFL";

        goto Test4;
      }

      ++local.Print.Index;
      local.Print.CheckSize();

      local.Print.Update.GlocalRptDtlLine.RptDetail =
        local.EabReportSend.RptDetail;

      if (Equal(export.EabFileHandling.Status, "FIELDNM"))
      {
        if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
        {
          export.EabFileHandling.Status = "PRTOVRFL";

          goto Test4;
        }

        ++local.Print.Index;
        local.Print.CheckSize();

        local.Print.Update.GlocalRptDtlLine.RptDetail = "FIELD NAME = " + local
          .Field.Name;
      }
      else if (Equal(export.EabFileHandling.Status, "SPECIAL") || Equal
        (export.EabFileHandling.Status, "EQUATION") || Equal
        (export.EabFileHandling.Status, "OPERAND1") || Equal
        (export.EabFileHandling.Status, "OPERATOR") || Equal
        (export.EabFileHandling.Status, "OPERAND2"))
      {
        if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
        {
          export.EabFileHandling.Status = "PRTOVRFL";

          goto Test4;
        }

        ++local.Print.Index;
        local.Print.CheckSize();

        local.Print.Update.GlocalRptDtlLine.RptDetail = "LINE REMAIN = " + local
          .DocumentLineRemain.Text80;

        if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
        {
          export.EabFileHandling.Status = "PRTOVRFL";

          goto Test4;
        }

        ++local.Print.Index;
        local.Print.CheckSize();

        local.Print.Update.GlocalRptDtlLine.RptDetail = "ORIGINAL    = " + local
          .DocumentLineOriginal.Text80;
      }
      else if (Equal(export.EabFileHandling.Status, "LOCSTORG"))
      {
        if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
        {
          export.EabFileHandling.Status = "PRTOVRFL";

          goto Test4;
        }

        ++local.Print.Index;
        local.Print.CheckSize();

        local.Print.Update.GlocalRptDtlLine.RptDetail =
          "OUT OF LOCAL STORAGE - Printing may cause loss of data";

        if (local.Print.Index + 2 > Local.PrintGroup.Capacity)
        {
          export.EabFileHandling.Status = "PRTOVRFL";

          goto Test4;
        }

        ++local.Print.Index;
        local.Print.CheckSize();

        local.Print.Update.GlocalRptDtlLine.RptDetail = "ORIGINAL    = " + local
          .DocumentLineOriginal.Text80;
      }

      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "+---+----1----+----2----+----3----+----4----+----5----+----6----+----7----+----8";
        
      UseCabErrorReport();
    }

Test4:

    // -----------------------------------------------------
    // Make sure the last page will have at least 10 lines
    // (each page requires a Benchmark for the mailing machine)
    // -----------------------------------------------------
    local.Print.Index = local.Print.Count - 1;
    local.Print.CheckSize();

    local.TempCalcCommon.Count = 1;

    while(!Equal(local.Print.Item.GlocalRptDtlLine.Command, "NEWPAGE"))
    {
      ++local.TempCalcCommon.Count;

      --local.Print.Index;
      local.Print.CheckSize();

      if (local.TempCalcCommon.Count >= local.MailingMachLinesIndent.Count)
      {
        break;
      }
    }

    if (local.TempCalcCommon.Count < local.MailingMachLinesIndent.Count)
    {
      local.Print.Count = local.Print.Count + local
        .MailingMachLinesIndent.Count - local.TempCalcCommon.Count;

      for(local.Print.Index = local.Print.Count - 1; local.Print.Index >= 0; --
        local.Print.Index)
      {
        if (!local.Print.CheckSize())
        {
          break;
        }

        if (!IsEmpty(local.Print.Item.GlocalRptDtlLine.Command))
        {
          break;
        }

        local.Print.Update.GlocalRptDtlLine.Command = "DETAIL";
      }

      local.Print.CheckIndex();
    }

    // -----------------------------------------------------
    // Determine the number of pages for the document
    // -----------------------------------------------------
    local.TempCalcCommon.Count = 0;

    for(local.Print.Index = 0; local.Print.Index < local.Print.Count; ++
      local.Print.Index)
    {
      if (!local.Print.CheckSize())
      {
        break;
      }

      if (Equal(local.Print.Item.GlocalRptDtlLine.Command, "NEWPAGE"))
      {
        ++local.TempCalcCommon.Count;
      }
    }

    local.Print.CheckIndex();

    if (Equal(export.EabFileHandling.Status, "OK"))
    {
      if (local.TempCalcCommon.Count > 7)
      {
        export.EabFileHandling.Status = "MAXPAGES";
      }
      else if (local.TempCalcCommon.Count < 1)
      {
        export.EabFileHandling.Status = "NOPAGES";
      }
      else
      {
      }
    }

    // -----------------------------------------------------
    // Determine to which output file the document should be written
    // -----------------------------------------------------
    local.EabReportSend.ReportNumber =
      local.DocumentTemplates.Item.GlocalReportNumber.ReportNumber;
    local.EabFileHandling.Action = "WRITE";
    local.Print.Index = 0;

    for(var limit = local.Print.Count; local.Print.Index < limit; ++
      local.Print.Index)
    {
      if (!local.Print.CheckSize())
      {
        break;
      }

      local.EabReportSend.Command = local.Print.Item.GlocalRptDtlLine.Command;
      local.EabReportSend.RptDetail =
        local.Print.Item.GlocalRptDtlLine.RptDetail;

      if (Equal(local.EabReportSend.Command, "NEWPAGE"))
      {
        local.PageLinesCount.Count = 0;
      }

      ++local.PageLinesCount.Count;

      if (local.PageLinesCount.Count < local.MailingMachLinesIndent.Count)
      {
        local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);

        switch(local.PageLinesCount.Count)
        {
          case 1:
            // -------------------------------------------
            // Mailer machine - INTEGRITY PAGE 4
            // -------------------------------------------
            if (local.TempCalcCommon.Count > 3)
            {
              if (local.Print.Index < 7)
              {
                local.FieldValue.Value = "     ____";
              }
            }

            break;
          case 2:
            // -------------------------------------------
            // Mailer machine - INTEGRITY PAGE 2
            // -------------------------------------------
            if (local.TempCalcCommon.Count == 2 || local
              .TempCalcCommon.Count == 3 || local.TempCalcCommon.Count == 6 || local
              .TempCalcCommon.Count == 7)
            {
              if (local.Print.Index < 7)
              {
                local.FieldValue.Value = "     ____";
              }
            }

            break;
          case 3:
            // -------------------------------------------
            // Mailer machine - INTEGRITY PAGE 1
            // -------------------------------------------
            if (local.TempCalcCommon.Count == 1 || local
              .TempCalcCommon.Count == 3 || local.TempCalcCommon.Count == 5 || local
              .TempCalcCommon.Count == 7)
            {
              if (local.Print.Index < 7)
              {
                local.FieldValue.Value = "     ____";
              }
            }

            break;
          case 4:
            // -------------------------------------------
            // Mailer machine - SELECT FEED 2
            // -------------------------------------------
            break;
          case 5:
            // -------------------------------------------
            // Mailer machine - SELECT FEED 1
            // -------------------------------------------
            break;
          case 6:
            // -------------------------------------------
            // Mailer machine - EXCEPTION
            // -------------------------------------------
            if (AsChar(import.Exception.Flag) == 'Y')
            {
              if (local.Print.Index < 7)
              {
                local.FieldValue.Value = "     ____";
              }
            }

            break;
          case 7:
            // -------------------------------------------
            // Mailer machine - END COLLATION
            // -------------------------------------------
            if (local.Print.Index < 7)
            {
              local.FieldValue.Value = "     ____";
            }

            break;
          case 8:
            // -------------------------------------------
            // Mailer machine - BENCHMARK
            // -------------------------------------------
            local.FieldValue.Value = "     ____";

            break;
          default:
            // -------------------------------------------
            // Blank lines after mailer machine marks
            // -------------------------------------------
            break;
        }

        if (!IsEmpty(local.FieldValue.Value))
        {
          local.EabReportSend.RptDetail =
            Substring(local.FieldValue.Value, 1, 9) + Substring
            (local.Null1.Text20, WorkArea.Text20_MaxLength, 1,
            local.MailingMachCharsIndent.Count - 9) + Substring
            (local.EabReportSend.RptDetail, EabReportSend.RptDetail_MaxLength,
            local.MailingMachCharsIndent.Count +
            1, Length(TrimEnd(local.EabReportSend.RptDetail)) -
            local.MailingMachCharsIndent.Count);
        }
      }

      if (Equal(export.EabFileHandling.Status, "OK"))
      {
        UseSpEabWriteDocument();

        if (!Equal(export.EabFileHandling.Status, "OK"))
        {
          export.EabFileHandling.Status = "FAILWRIT";

          return;
        }
      }
      else
      {
        UseCabErrorReport();
      }
    }

    local.Print.CheckIndex();
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.VersionNumber = source.VersionNumber;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.Command = source.Command;
    target.RptDetail = source.RptDetail;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseSpEabReadDocumentTemplate1()
  {
    var useImport = new SpEabReadDocumentTemplate.Import();
    var useExport = new SpEabReadDocumentTemplate.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.WorkArea.Text80 = local.DocumentLine.Text80;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(SpEabReadDocumentTemplate.Execute, useImport, useExport);

    local.DocumentLine.Text80 = useExport.WorkArea.Text80;
    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpEabReadDocumentTemplate2()
  {
    var useImport = new SpEabReadDocumentTemplate.Import();
    var useExport = new SpEabReadDocumentTemplate.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(SpEabReadDocumentTemplate.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpEabWriteDocument()
  {
    var useImport = new SpEabWriteDocument.Import();
    var useExport = new SpEabWriteDocument.Export();

    useImport.EabReportSend.Assign(local.EabReportSend);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(SpEabWriteDocument.Execute, useImport, useExport);

    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadDocumentField()
  {
    entities.DocumentField.Populated = false;

    return Read("ReadDocumentField",
      (db, command) =>
      {
        db.SetString(command, "fldName", entities.Field.Name);
        db.SetString(command, "docName", import.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          import.Document.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DocumentField.Position = db.GetInt32(reader, 0);
        entities.DocumentField.FldName = db.GetString(reader, 1);
        entities.DocumentField.DocName = db.GetString(reader, 2);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 3);
        entities.DocumentField.Populated = true;
      });
  }

  private bool ReadField()
  {
    entities.Field.Populated = false;

    return Read("ReadField",
      (db, command) =>
      {
        db.SetString(command, "name", local.Field.Name);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Populated = true;
      });
  }

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
        db.SetString(command, "docName", import.Document.Name);
        db.SetDate(
          command, "docEffectiveDte",
          import.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "fldName", local.Field.Name);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 1);
        entities.OutgoingDocument.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of Exception.
    /// </summary>
    [JsonPropertyName("exception")]
    public Common Exception
    {
      get => exception ??= new();
      set => exception = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Common exception;
    private Document document;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>A EquationNestingGroup group.</summary>
    [Serializable]
    public class EquationNestingGroup
    {
      /// <summary>
      /// A value of GlocalEquationResult.
      /// </summary>
      [JsonPropertyName("glocalEquationResult")]
      public Common GlocalEquationResult
      {
        get => glocalEquationResult ??= new();
        set => glocalEquationResult = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Common glocalEquationResult;
    }

    /// <summary>A PrintGroup group.</summary>
    [Serializable]
    public class PrintGroup
    {
      /// <summary>
      /// A value of GlocalRptDtlLine.
      /// </summary>
      [JsonPropertyName("glocalRptDtlLine")]
      public EabReportSend GlocalRptDtlLine
      {
        get => glocalRptDtlLine ??= new();
        set => glocalRptDtlLine = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 440;

      private EabReportSend glocalRptDtlLine;
    }

    /// <summary>A DocumentTemplatesGroup group.</summary>
    [Serializable]
    public class DocumentTemplatesGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Document G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of GlocalPageMaxLines.
      /// </summary>
      [JsonPropertyName("glocalPageMaxLines")]
      public Common GlocalPageMaxLines
      {
        get => glocalPageMaxLines ??= new();
        set => glocalPageMaxLines = value;
      }

      /// <summary>
      /// A value of GlocalMarginWidth.
      /// </summary>
      [JsonPropertyName("glocalMarginWidth")]
      public Common GlocalMarginWidth
      {
        get => glocalMarginWidth ??= new();
        set => glocalMarginWidth = value;
      }

      /// <summary>
      /// A value of GlocalReportNumber.
      /// </summary>
      [JsonPropertyName("glocalReportNumber")]
      public EabReportSend GlocalReportNumber
      {
        get => glocalReportNumber ??= new();
        set => glocalReportNumber = value;
      }

      /// <summary>
      /// Gets a value of SubDocumentLines.
      /// </summary>
      [JsonIgnore]
      public Array<SubDocumentLinesGroup> SubDocumentLines =>
        subDocumentLines ??= new(SubDocumentLinesGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of SubDocumentLines for json serialization.
      /// </summary>
      [JsonPropertyName("subDocumentLines")]
      [Computed]
      public IList<SubDocumentLinesGroup> SubDocumentLines_Json
      {
        get => subDocumentLines;
        set => SubDocumentLines.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document g;
      private Common glocalPageMaxLines;
      private Common glocalMarginWidth;
      private EabReportSend glocalReportNumber;
      private Array<SubDocumentLinesGroup> subDocumentLines;
    }

    /// <summary>A SubDocumentLinesGroup group.</summary>
    [Serializable]
    public class SubDocumentLinesGroup
    {
      /// <summary>
      /// A value of GlocalDocumentLine.
      /// </summary>
      [JsonPropertyName("glocalDocumentLine")]
      public WorkArea GlocalDocumentLine
      {
        get => glocalDocumentLine ??= new();
        set => glocalDocumentLine = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 440;

      private WorkArea glocalDocumentLine;
    }

    /// <summary>
    /// A value of MailingMachCharsIndent.
    /// </summary>
    [JsonPropertyName("mailingMachCharsIndent")]
    public Common MailingMachCharsIndent
    {
      get => mailingMachCharsIndent ??= new();
      set => mailingMachCharsIndent = value;
    }

    /// <summary>
    /// A value of MailingMachLinesIndent.
    /// </summary>
    [JsonPropertyName("mailingMachLinesIndent")]
    public Common MailingMachLinesIndent
    {
      get => mailingMachLinesIndent ??= new();
      set => mailingMachLinesIndent = value;
    }

    /// <summary>
    /// A value of DocmntLineOverflowWidth.
    /// </summary>
    [JsonPropertyName("docmntLineOverflowWidth")]
    public Common DocmntLineOverflowWidth
    {
      get => docmntLineOverflowWidth ??= new();
      set => docmntLineOverflowWidth = value;
    }

    /// <summary>
    /// A value of DocmntLineOverflow.
    /// </summary>
    [JsonPropertyName("docmntLineOverflow")]
    public WorkArea DocmntLineOverflow
    {
      get => docmntLineOverflow ??= new();
      set => docmntLineOverflow = value;
    }

    /// <summary>
    /// A value of NoSpace.
    /// </summary>
    [JsonPropertyName("noSpace")]
    public Common NoSpace
    {
      get => noSpace ??= new();
      set => noSpace = value;
    }

    /// <summary>
    /// A value of IndentParagraph.
    /// </summary>
    [JsonPropertyName("indentParagraph")]
    public Common IndentParagraph
    {
      get => indentParagraph ??= new();
      set => indentParagraph = value;
    }

    /// <summary>
    /// A value of TempCalcWorkArea.
    /// </summary>
    [JsonPropertyName("tempCalcWorkArea")]
    public WorkArea TempCalcWorkArea
    {
      get => tempCalcWorkArea ??= new();
      set => tempCalcWorkArea = value;
    }

    /// <summary>
    /// A value of EquationResultToSkip.
    /// </summary>
    [JsonPropertyName("equationResultToSkip")]
    public Common EquationResultToSkip
    {
      get => equationResultToSkip ??= new();
      set => equationResultToSkip = value;
    }

    /// <summary>
    /// Gets a value of EquationNesting.
    /// </summary>
    [JsonIgnore]
    public Array<EquationNestingGroup> EquationNesting =>
      equationNesting ??= new(EquationNestingGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of EquationNesting for json serialization.
    /// </summary>
    [JsonPropertyName("equationNesting")]
    [Computed]
    public IList<EquationNestingGroup> EquationNesting_Json
    {
      get => equationNesting;
      set => EquationNesting.Assign(value);
    }

    /// <summary>
    /// A value of EquationIgnoreLevel.
    /// </summary>
    [JsonPropertyName("equationIgnoreLevel")]
    public Common EquationIgnoreLevel
    {
      get => equationIgnoreLevel ??= new();
      set => equationIgnoreLevel = value;
    }

    /// <summary>
    /// A value of EquationOperator.
    /// </summary>
    [JsonPropertyName("equationOperator")]
    public Common EquationOperator
    {
      get => equationOperator ??= new();
      set => equationOperator = value;
    }

    /// <summary>
    /// A value of EquationOperand2.
    /// </summary>
    [JsonPropertyName("equationOperand2")]
    public WorkArea EquationOperand2
    {
      get => equationOperand2 ??= new();
      set => equationOperand2 = value;
    }

    /// <summary>
    /// A value of EquationOperand1.
    /// </summary>
    [JsonPropertyName("equationOperand1")]
    public WorkArea EquationOperand1
    {
      get => equationOperand1 ??= new();
      set => equationOperand1 = value;
    }

    /// <summary>
    /// A value of EquationOperand.
    /// </summary>
    [JsonPropertyName("equationOperand")]
    public Common EquationOperand
    {
      get => equationOperand ??= new();
      set => equationOperand = value;
    }

    /// <summary>
    /// A value of DocmntLineAfterCcWidth.
    /// </summary>
    [JsonPropertyName("docmntLineAfterCcWidth")]
    public Common DocmntLineAfterCcWidth
    {
      get => docmntLineAfterCcWidth ??= new();
      set => docmntLineAfterCcWidth = value;
    }

    /// <summary>
    /// A value of MailingMachineMarkWidth.
    /// </summary>
    [JsonPropertyName("mailingMachineMarkWidth")]
    public Common MailingMachineMarkWidth
    {
      get => mailingMachineMarkWidth ??= new();
      set => mailingMachineMarkWidth = value;
    }

    /// <summary>
    /// Gets a value of Print.
    /// </summary>
    [JsonIgnore]
    public Array<PrintGroup> Print => print ??= new(PrintGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Print for json serialization.
    /// </summary>
    [JsonPropertyName("print")]
    [Computed]
    public IList<PrintGroup> Print_Json
    {
      get => print;
      set => Print.Assign(value);
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of FieldLengthSpecification.
    /// </summary>
    [JsonPropertyName("fieldLengthSpecification")]
    public Common FieldLengthSpecification
    {
      get => fieldLengthSpecification ??= new();
      set => fieldLengthSpecification = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public WorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of DocumentLineRemain.
    /// </summary>
    [JsonPropertyName("documentLineRemain")]
    public WorkArea DocumentLineRemain
    {
      get => documentLineRemain ??= new();
      set => documentLineRemain = value;
    }

    /// <summary>
    /// A value of DocmntLineAfterCc.
    /// </summary>
    [JsonPropertyName("docmntLineAfterCc")]
    public WorkArea DocmntLineAfterCc
    {
      get => docmntLineAfterCc ??= new();
      set => docmntLineAfterCc = value;
    }

    /// <summary>
    /// A value of DocumentLineTemp.
    /// </summary>
    [JsonPropertyName("documentLineTemp")]
    public WorkArea DocumentLineTemp
    {
      get => documentLineTemp ??= new();
      set => documentLineTemp = value;
    }

    /// <summary>
    /// A value of DocumentLineOriginal.
    /// </summary>
    [JsonPropertyName("documentLineOriginal")]
    public WorkArea DocumentLineOriginal
    {
      get => documentLineOriginal ??= new();
      set => documentLineOriginal = value;
    }

    /// <summary>
    /// A value of DocumentLine.
    /// </summary>
    [JsonPropertyName("documentLine")]
    public WorkArea DocumentLine
    {
      get => documentLine ??= new();
      set => documentLine = value;
    }

    /// <summary>
    /// A value of CenterRptDtl.
    /// </summary>
    [JsonPropertyName("centerRptDtl")]
    public Common CenterRptDtl
    {
      get => centerRptDtl ??= new();
      set => centerRptDtl = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of SpecialCharacter.
    /// </summary>
    [JsonPropertyName("specialCharacter")]
    public WorkArea SpecialCharacter
    {
      get => specialCharacter ??= new();
      set => specialCharacter = value;
    }

    /// <summary>
    /// A value of NextPosition.
    /// </summary>
    [JsonPropertyName("nextPosition")]
    public Common NextPosition
    {
      get => nextPosition ??= new();
      set => nextPosition = value;
    }

    /// <summary>
    /// A value of PageLinesCount.
    /// </summary>
    [JsonPropertyName("pageLinesCount")]
    public Common PageLinesCount
    {
      get => pageLinesCount ??= new();
      set => pageLinesCount = value;
    }

    /// <summary>
    /// A value of PageMaxLines.
    /// </summary>
    [JsonPropertyName("pageMaxLines")]
    public Common PageMaxLines
    {
      get => pageMaxLines ??= new();
      set => pageMaxLines = value;
    }

    /// <summary>
    /// A value of RptDtlMaxWidth.
    /// </summary>
    [JsonPropertyName("rptDtlMaxWidth")]
    public Common RptDtlMaxWidth
    {
      get => rptDtlMaxWidth ??= new();
      set => rptDtlMaxWidth = value;
    }

    /// <summary>
    /// A value of RptDtlWidth.
    /// </summary>
    [JsonPropertyName("rptDtlWidth")]
    public Common RptDtlWidth
    {
      get => rptDtlWidth ??= new();
      set => rptDtlWidth = value;
    }

    /// <summary>
    /// A value of TempCalcCommon.
    /// </summary>
    [JsonPropertyName("tempCalcCommon")]
    public Common TempCalcCommon
    {
      get => tempCalcCommon ??= new();
      set => tempCalcCommon = value;
    }

    /// <summary>
    /// A value of DocumentLineRemainWidth.
    /// </summary>
    [JsonPropertyName("documentLineRemainWidth")]
    public Common DocumentLineRemainWidth
    {
      get => documentLineRemainWidth ??= new();
      set => documentLineRemainWidth = value;
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
    /// A value of DocumentNameVersion.
    /// </summary>
    [JsonPropertyName("documentNameVersion")]
    public WorkArea DocumentNameVersion
    {
      get => documentNameVersion ??= new();
      set => documentNameVersion = value;
    }

    /// <summary>
    /// A value of TemplateFileRead.
    /// </summary>
    [JsonPropertyName("templateFileRead")]
    public Common TemplateFileRead
    {
      get => templateFileRead ??= new();
      set => templateFileRead = value;
    }

    /// <summary>
    /// Gets a value of DocumentTemplates.
    /// </summary>
    [JsonIgnore]
    public Array<DocumentTemplatesGroup> DocumentTemplates =>
      documentTemplates ??= new(DocumentTemplatesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DocumentTemplates for json serialization.
    /// </summary>
    [JsonPropertyName("documentTemplates")]
    [Computed]
    public IList<DocumentTemplatesGroup> DocumentTemplates_Json
    {
      get => documentTemplates;
      set => DocumentTemplates.Assign(value);
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      mailingMachCharsIndent = null;
      mailingMachLinesIndent = null;
      docmntLineOverflowWidth = null;
      docmntLineOverflow = null;
      noSpace = null;
      indentParagraph = null;
      tempCalcWorkArea = null;
      equationResultToSkip = null;
      equationNesting = null;
      equationIgnoreLevel = null;
      equationOperator = null;
      equationOperand2 = null;
      equationOperand1 = null;
      equationOperand = null;
      docmntLineAfterCcWidth = null;
      mailingMachineMarkWidth = null;
      print = null;
      length = null;
      fieldLengthSpecification = null;
      null1 = null;
      document = null;
      documentLineRemain = null;
      docmntLineAfterCc = null;
      documentLineTemp = null;
      documentLineOriginal = null;
      documentLine = null;
      centerRptDtl = null;
      fieldValue = null;
      field = null;
      specialCharacter = null;
      nextPosition = null;
      pageLinesCount = null;
      pageMaxLines = null;
      rptDtlMaxWidth = null;
      rptDtlWidth = null;
      tempCalcCommon = null;
      documentLineRemainWidth = null;
      position = null;
      documentNameVersion = null;
      eabReportSend = null;
      eabFileHandling = null;
    }

    private Common mailingMachCharsIndent;
    private Common mailingMachLinesIndent;
    private Common docmntLineOverflowWidth;
    private WorkArea docmntLineOverflow;
    private Common noSpace;
    private Common indentParagraph;
    private WorkArea tempCalcWorkArea;
    private Common equationResultToSkip;
    private Array<EquationNestingGroup> equationNesting;
    private Common equationIgnoreLevel;
    private Common equationOperator;
    private WorkArea equationOperand2;
    private WorkArea equationOperand1;
    private Common equationOperand;
    private Common docmntLineAfterCcWidth;
    private Common mailingMachineMarkWidth;
    private Array<PrintGroup> print;
    private Common length;
    private Common fieldLengthSpecification;
    private WorkArea null1;
    private Document document;
    private WorkArea documentLineRemain;
    private WorkArea docmntLineAfterCc;
    private WorkArea documentLineTemp;
    private WorkArea documentLineOriginal;
    private WorkArea documentLine;
    private Common centerRptDtl;
    private FieldValue fieldValue;
    private Field field;
    private WorkArea specialCharacter;
    private Common nextPosition;
    private Common pageLinesCount;
    private Common pageMaxLines;
    private Common rptDtlMaxWidth;
    private Common rptDtlWidth;
    private Common tempCalcCommon;
    private Common documentLineRemainWidth;
    private Common position;
    private WorkArea documentNameVersion;
    private Common templateFileRead;
    private Array<DocumentTemplatesGroup> documentTemplates;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ZdelRecordedDocument.
    /// </summary>
    [JsonPropertyName("zdelRecordedDocument")]
    public ZdelRecordedDocument ZdelRecordedDocument
    {
      get => zdelRecordedDocument ??= new();
      set => zdelRecordedDocument = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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

    private Infrastructure infrastructure;
    private ZdelRecordedDocument zdelRecordedDocument;
    private OutgoingDocument outgoingDocument;
    private Field field;
    private Document document;
    private DocumentField documentField;
    private FieldValue fieldValue;
  }
#endregion
}
