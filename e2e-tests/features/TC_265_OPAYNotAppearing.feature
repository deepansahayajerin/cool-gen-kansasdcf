Feature: TC_265_OPAY_NOT_APPEARING

  Background: Navigation
    Given Go to the ksdcf website
    Then snapshot "landing_page"
    Then "CSE - Main Menu" window appears

  Scenario Outline: Scenario1
    When user enters "LACS" in a field next to the 'NEXT'
    And presses "Enter"
    Then "CSE - List Legal Actions by CSE Case Number - LACS" window appears
    And enters "<CSE_CASE_NUMBRT>" in "CSE Case Number"
    When user enters "LACT" in a field next to the 'NEXT'
    And presses "Enter"
    Then "CSE - Legal Action - LACT" window appears

    When user enter "<COURT_CASE_NUMBER>" in "Court Case Number"
    And enter "<CLASSIFICATION>" in "Classification"
    And enter "COMP" in a field next to the 'NEXT'
    And presses "Enter"
    Then "CSE - Case Composition - COMP" window appears

    When user enters "LACT" in a field next to the 'NEXT'
    And presses "Enter"
    Then "CSE - Legal Action - LACT" window appears

    When user enter "<COURT_CASE_NUMBER>" in "Court Case Number"
    And enter "<CLASSIFICATION>" in "Classification"
    And enter "<TRIB_STATER>" in "Trib State"
    When user enter "<COUNTRY>" in third field above "Pay to"

    When user enter "S" in field below "CO Change on Date"
    And presses "F4"
    Then "CSE - Code Values List - CDVL" window appears

    When user enter "S" in 6th field below "Sel"
    And user presses "F9"
    Then "CSE - Legal Action - LACT" window appears

    Then snapshot "Sc1_1"

    When user enter "<LEG_ACT>" in first field above "Type"
    And enter "<ORDER_AUTHORITY>" in "Order Authority"
    And enter "<TYPE>" in "Type"
    And enter "<PMT_LOC>" in "Pmt Loc"
    Then snapshot "Sc1_2"

    #1. ???
    Then "CSE - Court Caption - CAPT" window appears

    When user enter "<CAPTION_LINES_1>" in first field below "Caption Lines"
    When user enter "<CAPTION_LINES_2>" in second field below "Caption Lines"
    When user enter "<CAPTION_LINES_3>" in third field below "Caption Lines"

    #2. ???
    Then "CSE - Legal Role - LROL" window appears
    When user enter "<CSE_CASE_NUMBRT>" in "CSE Case #s"

    #3. ???
    And enter "S" in first field below "Sel"
    And enter "R" in the next field
    And enter "S" in second field below "Sel"
    And enter "P" in the next field
    And enter "S" in third field below "Sel"
    And enter "C" in the next field

    #4. ???
    Then "CSE - Legal Action - LACT" window appears
    When user enter "<FILED_DATE>" in Filed Date

    #5. ???
    Then "CSE - Legal Detail - LDET" window appears
    When user enter "S" in first field below "Sel"
    And enter "<FN>" in the next field
    And enter "<LDET_TYPE>" in the next field
    And enter "<EFF_DATE>" in the next field
    And enter "<JUDGEMENT>" in the 4th field below "Pymt Locn"
    And enter "<DESCRIPTION>" in the 5th field below "Pymt Locn"

    #6. ???
    Then "CSE - Legal Obligation Persons - LOPS" window appears
    When user enter "S" in first field below "Sel"
    And enter "<RES_1>" in first field below "R/E/S"
    And enter "<JUDG_AMT_1>" in first field below "Judg Amt"

    When user enter "S" in third field below "Sel"
    And enter "<RES_2>" in third field below "R/E/S"
    And enter "<JUDG_AMT_2>" in third field below "Judg Amt"

    #7. ???
    Then "CSE - Legal Detail - LDET" window appears
    When user enter "S" in first field below "Sel"

    #8. ???
    Then "CSE - Maintain Non-Accruing Obligation - ONAC" window appears
    When user enter "<COV_PERIOD_FROM>" in "Cov Period"
    When user enter "<COV_PERIOD_TO>" in the next field
    And enter "<REASON>" in "Note / Contingency Reason"
    And enter "S" in first field below "Sel"
    And enter "<PROG>" in "Prog"

    #9. ???
    Then "CSE - List Obligations by AP/Payor - OPAY" window appears

    Examples:
      | CSE_CASE_NUMBRT | COURT_CASE_NUMBER | CLASSIFICATION | TRIB_STATER | COUNTRY | LEG_ACT | ORDER_AUTHORITY | TYPE | PMT_LOC | CAPTION_LINES_1 | CAPTION_LINES_2 | CAPTION_LINES_3 | FILED_DATE | FN | LDET_TYPE | EFF_DATE | JUDGEMENT | DESCRIPTION | RES_1 | JUDG_AMT_1 | RES_2 | JUDG_AMT_2 | COV_PERIOD_FROM | COV_PERIOD_TO | REASON | PROG |
      | 566553          | 22dm265           | J              | KS          | WY      | PR      | J               | P    | PAYCTR  | TREVIN POST     | VS              | SOMEBODY        | 05012022   | F  | AJ        | 05012022 | 400       | TC 265      | R     | 400        | S     | 400        | 02012022        | 04302022      | TX 265 | NA   |


