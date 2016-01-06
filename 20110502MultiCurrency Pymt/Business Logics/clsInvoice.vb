Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Text
Imports System
Public Class clsInvoice
    Inherits clsBase
    Private oCFLEvent As SAPbouiCOM.IChooseFromListEvent
    Private oDBSrc_Line As SAPbouiCOM.DBDataSource
    Private oMatrix As SAPbouiCOM.Matrix
    Private oEditText As SAPbouiCOM.EditText
    Private oComboBox As SAPbouiCOM.ComboBox
    Private oEditTextColumn As SAPbouiCOM.EditTextColumn
    Private oStatictext As SAPbouiCOM.StaticText
    Private oGrid, oGrid1 As SAPbouiCOM.Grid
    Private dtTemp As SAPbouiCOM.DataTable
    Private dtResult As SAPbouiCOM.DataTable
    Private oMode As SAPbouiCOM.BoFormMode
    Private oItem As SAPbobsCOM.Items
    Private oInvoice As SAPbobsCOM.Documents
    'Private InvBase As DocumentType
    Private InvBaseDocNo As String
    Private strCurrency, strLocalCurrency, strBPCurrency, strDocCurrency As String
    Private InvForConsumedItems As Integer
    Dim LCAmount As Double
    Dim LCExangeRage As Double
    Private blnErrorLog As Boolean = False
    Private dtPostingdate As Date
    Public Sub New()
        MyBase.New()
        InvForConsumedItems = 0
    End Sub
    Private Sub LoadForm()
        oForm = oApplication.Utilities.LoadForm(xml_BatchOrders, "frm_MultiCurrency")
        oForm = oApplication.SBO_Application.Forms.ActiveForm()
        AddChooseFromList(oForm)
        Databind(oForm)
    End Sub

#Region "AddCFL"
    Private Sub AddChooseFromList(ByVal aform As SAPbouiCOM.Form)
        Try
            Dim oCFLs As SAPbouiCOM.ChooseFromListCollection
            Dim oCons As SAPbouiCOM.Conditions
            Dim oCon As SAPbouiCOM.Condition

            oCFLs = aform.ChooseFromLists
            Dim oCFL As SAPbouiCOM.ChooseFromList
            Dim oCFLCreationParams As SAPbouiCOM.ChooseFromListCreationParams
            oCFLCreationParams = oApplication.SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams)

            ' Adding Header GL CFL, one for the edit text.
            oCFLCreationParams.MultiSelection = False
            oCFLCreationParams.ObjectType = "2"
            oCFLCreationParams.UniqueID = "CFL1"
            oCFL = oCFLs.Add(oCFLCreationParams)

            oCons = oCFL.GetConditions()

            oCon = oCons.Add()
            oCon.Alias = "CardType"
            oCon.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
            oCon.CondVal = "C"
            oCFL.SetConditions(oCons)

            oCFLCreationParams.MultiSelection = False
            oCFLCreationParams.ObjectType = "1"
            oCFLCreationParams.UniqueID = "CFL2"
            oCFL = oCFLs.Add(oCFLCreationParams)



          

            oCons = oCFL.GetConditions
            oCon = oCons.Add()
            oCon.Alias = "LocManTran"
            oCon.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
            oCon.CondVal = "N"
            oCFL.SetConditions(oCons)


            oCFLCreationParams.MultiSelection = False
            oCFLCreationParams.ObjectType = "1"
            oCFLCreationParams.UniqueID = "CFL3"
            oCFL = oCFLs.Add(oCFLCreationParams)


            oCons = oCFL.GetConditions()

            oCon = oCons.Add()
            oCon.Alias = "LocManTran"
            oCon.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
            oCon.CondVal = "N"
            oCFL.SetConditions(oCons)





        Catch
            oApplication.Utilities.Message(Err.Description, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub

#Region "DataBind"
    Private Sub Databind(ByVal aForm As SAPbouiCOM.Form)
        aForm.DataSources.UserDataSources.Add("BPCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT)
        aForm.DataSources.UserDataSources.Add("BPName", SAPbouiCOM.BoDataType.dt_LONG_TEXT)
        aForm.DataSources.UserDataSources.Add("GLAcct", SAPbouiCOM.BoDataType.dt_SHORT_TEXT)
        aForm.DataSources.UserDataSources.Add("TransDate", SAPbouiCOM.BoDataType.dt_DATE)
        aForm.DataSources.UserDataSources.Add("DocDate", SAPbouiCOM.BoDataType.dt_DATE)

        oEditText = aForm.Items.Item("7").Specific
        oEditText.DataBind.SetBound(True, "", "BPCode")
        oEditText.ChooseFromListUID = "CFL1"
        oEditText.ChooseFromListAlias = "CardCode"
        oEditText = aForm.Items.Item("9").Specific
        oEditText.DataBind.SetBound(True, "", "BPName")
        oEditText = aForm.Items.Item("13").Specific

        oEditText.DataBind.SetBound(True, "", "GLAcct")
        oEditText.ChooseFromListUID = "CFL2"
        oEditText.ChooseFromListAlias = "FormatCode"
        oEditText = aForm.Items.Item("15").Specific
        oEditText.DataBind.SetBound(True, "", "TransDate")
        oEditText = aForm.Items.Item("23").Specific
        oEditText.DataBind.SetBound(True, "", "DocDate")

        Dim oComboColum As SAPbouiCOM.ComboBoxColumn
        Dim oTempRec As SAPbobsCOM.Recordset
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery("Select CurrCode,CurrName from OCRN ")
        oGrid = aForm.Items.Item("10").Specific
        oGrid.DataTable.Rows.Add()
        oComboColum = oGrid.Columns.Item(0)
        oComboColum.ValidValues.Add("-", "-")
        For intRow As Integer = 0 To oTempRec.RecordCount - 1
            oComboColum.ValidValues.Add(oTempRec.Fields.Item(0).Value, oTempRec.Fields.Item(1).Value)
            oTempRec.MoveNext()
        Next

        oEditTextColumn = oGrid.Columns.Item("ActCode")

        oEditTextColumn.ChooseFromListUID = "CFL3"
        oEditTextColumn.ChooseFromListAlias = "FormatCode"
        oEditTextColumn.LinkedObjectType = "1"

        oComboBox = aForm.Items.Item("20").Specific
        oComboBox.ValidValues.Add("-", "-")
        oComboBox.ValidValues.Add("Cash", "Cash Payment")
        oComboBox.ValidValues.Add("Transfer", "Bank Transfer")
        oComboBox.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
        aForm.Items.Item("20").DisplayDesc = True

        oGrid = aForm.Items.Item("6").Specific
        dtTemp = oGrid.DataTable
        Dim strSQL As String
        'strSQL = "Select DocEntry,DocNum,DocDate,DocDueDate,DocTotal,PaidToDate,DocTotal-PaidTodate 'Balance',DocCur, DocTotal-DocTotal 'AppliedAmt' from OINV where DocTotal > PaidToDate and 1=2 order by docDueDate "
        'dtTemp.ExecuteQuery("SELECT T0.[CardCode], T0.[CardName], T1.[ItemCode], T1.[Dscription], T0.[DocDate],1 'Count' FROM ORDR T0  INNER JOIN RDR1 T1 ON T0.DocEntry = T1.DocEntry where 1=2")
        strSQL = "Select DocEntry,DocNum,DocDate,DocDueDate,DocTotal 'DocTotal(LC)',DocTotalFC 'DocTotal(FC)' ,PaidToDate ,PaidFC,DocTotal-PaidTodate 'Balance(LC)',DocTotalFC-PaidFC 'Balance FC',DocCur, DocTotal-DocTotal 'AppliedAmt' from OINV where DocTotal > PaidToDate and 1=2 order by docDueDate"
        dtTemp.ExecuteQuery(strSQL)
        oGrid.DataTable = dtTemp
        FormatGrid(oGrid)
    End Sub
#End Region
#Region "Clear Currency Matrix"
    Private Sub ClearCurrencyMatrix(ByVal aForm As SAPbouiCOM.Form)
        oGrid = aForm.Items.Item("10").Specific
        oGrid.DataTable.Rows.Clear()
        oGrid.DataTable.Rows.Add()
    End Sub
#End Region

#Region "Get Documents"
    Private Sub GetDocuments(ByVal strBPCode As String, ByVal aForm As SAPbouiCOM.Form)
        Dim strSQL As String
        'strSQL = "Select DocEntry,DocNum,DocDate,DocDueDate,DocTotal ,PaidToDate,DocTotal-PaidTodate 'Balance',DocCur, DocTotal-DocTotal 'AppliedAmt' from OINV where DocTotal > PaidToDate and cardcode ='" & strBPCode & "' order by docDueDate "
        strSQL = "Select DocEntry,DocNum,DocDate,DocDueDate,DocTotal 'DocTotal(LC)',DocTotalFC 'DocTotal(FC)' ,PaidToDate ,PaidFC,DocTotal-PaidTodate 'Balance(LC)',DocTotalFC-PaidFC 'Balance FC',DocCur, DocTotal-DocTotal 'AppliedAmt' from OINV where DocTotal > PaidToDate and cardcode ='" & strBPCode & "' order by docDueDate,docentry,docnum "
        oGrid = aForm.Items.Item("6").Specific
        dtTemp = oGrid.DataTable
        dtTemp.ExecuteQuery(strSQL)
        oGrid.DataTable = dtTemp
        FormatGrid(oGrid)
    End Sub
#End Region

    Private Sub FormatGrid(ByVal aGrid As SAPbouiCOM.Grid)
        aGrid.Columns.Item(0).TitleObject.Caption = "DocEntry"
        aGrid.Columns.Item(0).Editable = False
        oEditTextColumn = aGrid.Columns.Item(0)
        oEditTextColumn.LinkedObjectType = "13"
        aGrid.Columns.Item(1).TitleObject.Caption = "Doc Num"
        aGrid.Columns.Item(1).Editable = False
        aGrid.Columns.Item(2).TitleObject.Caption = "Doc Date"
        aGrid.Columns.Item(2).Editable = False
        oEditTextColumn = aGrid.Columns.Item(2)
        'oEditTextColumn.LinkedObjectType = "4"
        aGrid.Columns.Item(3).TitleObject.Caption = "DueDate"
        aGrid.Columns.Item(3).Editable = False
        aGrid.Columns.Item(4).TitleObject.Caption = "DocTotal (LC)"
        aGrid.Columns.Item(4).Editable = False
        aGrid.Columns.Item(5).TitleObject.Caption = "DocTotal FC"
        aGrid.Columns.Item(5).Editable = False
        aGrid.Columns.Item(6).TitleObject.Caption = "Amount Paid (LC)"
        aGrid.Columns.Item(6).Editable = False
        aGrid.Columns.Item(7).TitleObject.Caption = "Amount Paid (FC)"
        aGrid.Columns.Item(7).Editable = False

        aGrid.Columns.Item(8).TitleObject.Caption = "Balance Paid(LC)"
        aGrid.Columns.Item(8).Editable = False
        aGrid.Columns.Item(9).TitleObject.Caption = "Balance Paid(FC)"
        aGrid.Columns.Item(9).Editable = False
        aGrid.Columns.Item(10).TitleObject.Caption = "Currency"
        aGrid.Columns.Item(10).Editable = False

        aGrid.Columns.Item(11).TitleObject.Caption = "Applied Amount"
        aGrid.Columns.Item(11).Editable = True
        aGrid.AutoResizeColumns()
        aGrid.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_None
    End Sub

    Private Sub DeleteRow(ByVal aGrid As SAPbouiCOM.Grid)
        For intRow As Integer = 0 To aGrid.DataTable.Rows.Count - 1
            If aGrid.Rows.IsSelected(intRow) Then
                aGrid.DataTable.Rows.Remove(intRow)
                Exit Sub
            End If
        Next
    End Sub
#End Region

#Region "Get Document Number"
    Private Function GetDocNumber(ByVal aDocEntry As Integer) As String
        Dim oTempRs As SAPbobsCOM.Recordset
        oTempRs = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRs.DoQuery("select docnum from oinv where docentry=" & aDocEntry)
        Return oTempRs.Fields.Item(0).Value
    End Function
#End Region

#Region "Add in Sequence"
    Private Sub AddinSequence(ByVal aForm As SAPbouiCOM.Form)
        Dim dblExchanagerate, strFCCurrency, strPostingdate As String
        Dim dblFCAmount, dblCummulativeLCAmount, dblLCAMount, dblFCBalance As Double
        oGrid = aForm.Items.Item("10").Specific
        oGrid1 = aForm.Items.Item("6").Specific
        strLocalCurrency = oApplication.Utilities.GetCurrency("Local")
        strBPCurrency = oApplication.Utilities.GetCurrency("BP", oApplication.Utilities.getEditTextvalue(aForm, "7"))
        strPostingdate = oApplication.Utilities.getEditTextvalue(aForm, "23")
        If strPostingdate <> "" Then
            dtPostingdate = aForm.DataSources.UserDataSources.Item("DocDate").Value '  oApplication.Utilities.GetDateTimeValue(strdate)
        End If
        If strLocalCurrency <> strBPCurrency Then
            If strPostingdate = "" Then
                dblExchanagerate = oApplication.Utilities.getExchangeRate(strBPCurrency)
            Else
                dblExchanagerate = oApplication.Utilities.getExchangeRate(strBPCurrency, dtPostingdate)
            End If
        Else
            dblExchanagerate = 1
        End If

        dblCummulativeLCAmount = 0
        Dim dblExchangerate1 As Double
        For intCurrrow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            strFCCurrency = oGrid.DataTable.GetValue(0, intCurrrow)
            dblLCAMount = 0
            If strFCCurrency <> "" Then
                If strFCCurrency = strLocalCurrency Then
                    dblExchangerate1 = 1
                    oGrid.DataTable.SetValue(2, intCurrrow, oGrid.DataTable.GetValue(1, intCurrrow))
                    oGrid.DataTable.SetValue(3, intCurrrow, dblExchangerate1)
                Else
                    dblFCAmount = oGrid.DataTable.GetValue(1, intCurrrow)
                    'dblExchangerate1 = oApplication.Utilities.getExchangeRate(strFCCurrency)
                    If strPostingdate = "" Then
                        dblExchangerate1 = oApplication.Utilities.getExchangeRate(strFCCurrency)
                    Else
                        dblExchangerate1 = oApplication.Utilities.getExchangeRate(strFCCurrency, dtPostingdate)
                    End If
                    'dblFCAmount = dblFCAmount * dblExchangerate1
                    If getPaymentMethod() = True Then
                        dblFCAmount = dblFCAmount * dblExchangerate1
                    Else
                        dblFCAmount = dblFCAmount / dblExchangerate1
                    End If

                    oGrid.DataTable.SetValue(2, intCurrrow, dblFCAmount)
                    oGrid.DataTable.SetValue(3, intCurrrow, dblExchangerate1)
                End If
                dblLCAMount = oGrid.DataTable.GetValue(2, intCurrrow)
                dblCummulativeLCAmount = dblCummulativeLCAmount + dblLCAMount
            End If
        Next

        Dim strSQL, strBPCode As String
        Dim dblCum1, dbldocvalue As Double
        dblCum1 = dblCummulativeLCAmount
        Dim oTempRS As SAPbobsCOM.Recordset
        oTempRS = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        strBPCode = oApplication.Utilities.getEditTextvalue(aForm, "7")
        '   strSQL = "Select DocEntry,DocNum,DocDate,DocDueDate,DocTotal,PaidToDate,DocTotal-PaidTodate 'Balance',DocCur, DocTotal-DocTotal 'AppliedAmt' from OINV where DocTotal > PaidToDate and cardcode ='" & strBPCode & "' order by docDueDate "
        strSQL = "Select DocEntry,DocNum,DocDate,DocDueDate,DocTotal 'DocTotal(LC)',DocTotalFC 'DocTotal(FC)' ,PaidToDate ,PaidFC,DocTotal-PaidTodate 'Balance(LC)',DocTotalFC-PaidFC 'Balance FC',DocCur, DocTotal-DocTotal 'AppliedAmt' from OINV where DocTotal > PaidToDate and cardcode ='" & strBPCode & "'  order by docDueDate"
        oTempRS.DoQuery("delete from [@DABT_PayTemp]")
        oTempRS.DoQuery(strSQL)
        Dim oUserTable As SAPbobsCOM.UserTable
        Dim strCode As String
        dblCum1 = Math.Round(dblCum1, 2)
        For intRow As Integer = 0 To oTempRS.RecordCount - 1
            If dblCum1 > 0 Then
                dbldocvalue = oTempRS.Fields.Item(8).Value
                If dbldocvalue <= dblCum1 Then
                    dbldocvalue = dbldocvalue
                Else
                    dbldocvalue = dblCum1
                End If
                oUserTable = oApplication.Company.UserTables.Item("DABT_PayTemp")
                strCode = oApplication.Utilities.getMaxCode("@DABT_PayTemp", "Code")
                oUserTable.Code = strCode
                oUserTable.Name = strCode
                oUserTable.UserFields.Fields.Item("U_CardCode").Value = strBPCode
                oUserTable.UserFields.Fields.Item("U_DocEntry").Value = oTempRS.Fields.Item(0).Value
                If oUserTable.Add <> 0 Then
                    MsgBox(oApplication.Company.GetLastErrorDescription)
                End If
            Else
                Exit For
            End If
            dblCum1 = dblCum1 - dbldocvalue
            dblCum1 = Math.Round(dblCum1, 2)

            oTempRS.MoveNext()
        Next
        'strSQL = "Select DocEntry,DocNum,DocDate,DocDueDate,DocTotal,PaidToDate,DocTotal-PaidTodate 'Balance',DocCur, DocTotal-DocTotal 'AppliedAmt' from OINV where DocTotal > PaidToDate and cardcode ='" & strBPCode & "' and DocEntry in (Select U_DocEntry from [@DABT_PayTemp]) order by docDueDate "
        strSQL = "Select DocEntry,DocNum,DocDate,DocDueDate,DocTotal 'DocTotal(LC)',DocTotalFC 'DocTotal(FC)' ,PaidToDate ,PaidFC,DocTotal-PaidTodate 'Balance(LC)',DocTotalFC-PaidFC 'Balance FC',DocCur, DocTotal-DocTotal 'AppliedAmt' from OINV where DocTotal > PaidToDate and cardcode ='" & strBPCode & "' and  DocEntry in (Select U_DocEntry from [@DABT_PayTemp]) order by docDueDate  "
        oGrid = aForm.Items.Item("6").Specific
        dtTemp = oGrid.DataTable
        dtTemp.ExecuteQuery(strSQL)
        oGrid.DataTable = dtTemp
        FormatGrid(oGrid)
        Dim dblPayAmount, dblRowAmt, dblAppliedamt, dblpaidtodate, dblBalance As Double
        Dim strCurrency, strDocCurrency As String
        oGrid = aForm.Items.Item("10").Specific
        oGrid1 = aForm.Items.Item("6").Specific
        Dim oTemprs1 As SAPbobsCOM.Recordset
        oTemprs1 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTemprs1.DoQuery("delete from [@DABT_PayTemp]")
        oTemprs1.DoQuery(strSQL)
        For intPay As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            dblPayAmount = oGrid.DataTable.GetValue(2, intPay)
            dblPayAmount = Math.Round(dblPayAmount, 2)
            If dblPayAmount > 0 Then
                For intDoc As Integer = 0 To oGrid1.DataTable.Rows.Count - 1
                    dblRowAmt = oGrid1.DataTable.GetValue("Balance(LC)", intDoc)
                    dblRowAmt = Math.Round(dblRowAmt, 2)
                    dblAppliedamt = oGrid1.DataTable.GetValue("AppliedAmt", intDoc)
                    dblAppliedamt = Math.Round(dblAppliedamt, 2)
                    dblRowAmt = dblRowAmt - dblAppliedamt
                    If dblRowAmt > 0 And dblPayAmount > 0 Then
                        If dblRowAmt <= dblPayAmount Then
                            dblRowAmt = dblRowAmt
                            dblPayAmount = dblPayAmount - dblRowAmt
                        Else
                            dblRowAmt = dblPayAmount
                            dblPayAmount = dblPayAmount - dblRowAmt
                        End If
                        strCurrency = oGrid.DataTable.GetValue(0, intPay)
                        strDocCurrency = oGrid1.DataTable.GetValue("DocCur", intDoc)
                        oGrid1.DataTable.SetValue("AppliedAmt", intDoc, dblRowAmt)
                        oGrid1.DataTable.SetValue("DocCur", intDoc, strCurrency)
                        Dim intDocEntry As Integer
                        intDocEntry = oGrid1.DataTable.GetValue(0, intDoc)
                        dblBalance = oGrid1.DataTable.GetValue("Balance(LC)", intDoc)
                        dblFCBalance = oGrid1.DataTable.GetValue("Balance FC", intDoc)
                        oUserTable = oApplication.Company.UserTables.Item("DABT_PayTemp")
                        strCode = oApplication.Utilities.getMaxCode("@DABT_PayTemp", "Code")
                        oUserTable.Code = strCode
                        oUserTable.Name = strCode
                        oUserTable.UserFields.Fields.Item("U_Currency").Value = strCurrency
                        oUserTable.UserFields.Fields.Item("U_DocEntry").Value = intDocEntry
                        'If strCurrency <> strDocCurrency Then
                        '    dblExchanagerate = oApplication.Utilities.getExchangeRate(strDocCurrency)
                        '    dblRowAmt = dblRowAmt / dblExchanagerate
                        'End If
                        oUserTable.UserFields.Fields.Item("U_AppAmt").Value = dblRowAmt
                        oUserTable.UserFields.Fields.Item("U_Balance").Value = dblBalance
                        oUserTable.UserFields.Fields.Item("U_BalFC").Value = dblFCBalance
                        oUserTable.UserFields.Fields.Item("U_DocNum").Value = GetDocNumber(intDocEntry)
                        If oUserTable.Add <> 0 Then
                            MsgBox(oApplication.Company.GetLastErrorDescription)
                        End If
                    End If
                Next
            End If
        Next
        strSQL = "Select U_DocEntry,U_docNum,U_Balance,U_BalFC,U_Currency,U_AppAmt from [@DABT_PayTemp]"
        oGrid = aForm.Items.Item("6").Specific
        dtTemp = oGrid.DataTable
        dtTemp.ExecuteQuery(strSQL)
        oGrid.DataTable = dtTemp
        aForm.Items.Item("6").Enabled = False
        oGrid.Columns.Item(0).TitleObject.Caption = "Invoice Number"
        Dim oeditTextColumn As SAPbouiCOM.EditTextColumn
        oeditTextColumn = oGrid.Columns.Item(0)
        oeditTextColumn.LinkedObjectType = "13"
        oGrid.Columns.Item(1).TitleObject.Caption = "Document Number"
        oGrid.Columns.Item(2).TitleObject.Caption = "Balance Amount (LC)"
        oGrid.Columns.Item(3).TitleObject.Caption = "Balance Amount (FC)"
        oGrid.Columns.Item(4).TitleObject.Caption = "Document Currency"
        oGrid.Columns.Item(5).TitleObject.Caption = "Applied Amount (LC)"
    End Sub
#End Region

#Region "Get Account Code"
    Private Function getPaymentMethod() As Boolean

        Dim oTempRec As SAPbobsCOM.Recordset
        Dim stNumAtCard As String
        Dim dtNumatcard As Decimal
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery("select DirectRate from OADM")
        stNumAtCard = oTempRec.Fields.Item(0).Value
        If oTempRec.Fields.Item(0).Value = "Y" Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Function getAcctCode(ByVal aBPCode As String) As String
        Dim oTempRec As SAPbobsCOM.Recordset
        Dim stNumAtCard As String
        Dim dtNumatcard As Decimal
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery("select isnull(AcctCode,'') from OACT where Formatcode='" & aBPCode & "'")
        stNumAtCard = oTempRec.Fields.Item(0).Value
        Return stNumAtCard
    End Function
    Private Function getAcctCurrency(ByVal aBPCode As String) As String
        Dim oTempRec As SAPbobsCOM.Recordset
        Dim stNumAtCard As String
        Dim dtNumatcard As Decimal
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery("select ActCurr from OACT where Formatcode='" & aBPCode & "'")
        stNumAtCard = oTempRec.Fields.Item(0).Value
        Return stNumAtCard
    End Function
    Private Function getBPCurrency(ByVal aBPCode As String) As String
        Dim oTempRec As SAPbobsCOM.Recordset
        Dim stNumAtCard As String
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery("select currency from ocrd where cardcode='" & aBPCode & "'")
        stNumAtCard = oTempRec.Fields.Item(0).Value
        Return stNumAtCard
    End Function
#End Region

#Region "Validation"
    Private Function Validation(ByVal aForm As SAPbouiCOM.Form) As Boolean
        Dim strBPCurrency, strcardcode, strAccountCurrency, strAccount, strMainCurrency, strCurrency, strdate, strpostingdate As String
        Dim blnMulticurrency As Boolean = False
        Dim dtDate As Date
        Dim intCopies As Integer
        Dim dblAmount As Double
        Dim dtTransDate As Date
        Dim oTempRec As SAPbobsCOM.Recordset

        strAccountCurrency = getAcctCurrency(oApplication.Utilities.getEditTextvalue(aForm, "13"))
        strBPCurrency = getBPCurrency(oApplication.Utilities.getEditTextvalue(aForm, "7"))
        oGrid = aForm.Items.Item("10").Specific
        dblAmount = 0
        strCurrency = ""
        strMainCurrency = ""
        strcardcode = oApplication.Utilities.getEditTextvalue(aForm, "7")
        strdate = oApplication.Utilities.getEditTextvalue(aForm, "15")
        strAccount = oApplication.Utilities.getEditTextvalue(aForm, "13")
        strpostingdate = oApplication.Utilities.getEditTextvalue(aForm, "23")

        If strpostingdate <> "" Then
            dtPostingdate = aForm.DataSources.UserDataSources.Item("DocDate").Value '  oApplication.Utilities.GetDateTimeValue(strdate)
        Else
            oApplication.Utilities.Message("Posting date is missing..", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If

        If strcardcode = "" Then
            oApplication.Utilities.Message("Business Partner code is missing..", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If
        oComboBox = aForm.Items.Item("20").Specific
        If oComboBox.Selected.Value = "-" Then
            oApplication.Utilities.Message("Payment type is missing...", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If
        'If strAccount = "" Then
        '    oApplication.Utilities.Message("Transfer account is missing..", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        '    Return False
        'End If
        oComboBox = aForm.Items.Item("20").Specific
        If oComboBox.Selected.Value = "Transfer" Then
            If strdate = "" Then
                oApplication.Utilities.Message("Transfer Date is missing...", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            End If
        End If

        If oGrid.DataTable.Rows.Count < 2 Then
            oApplication.Utilities.Message("Transfer amount details are missing", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If

        For intRow As Integer = oGrid.DataTable.Rows.Count - 1 To 0 Step -1
            If oGrid.DataTable.GetValue(0, intRow) = "-" Then
                oGrid.DataTable.Rows.Remove(intRow)
            End If
        Next

        For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            strMainCurrency = oGrid.DataTable.GetValue(0, intRow)
            If strMainCurrency <> "" Then
                For intLoop As Integer = 0 To oGrid.DataTable.Rows.Count - 1
                    If intRow <> intLoop Then
                        strCurrency = oGrid.DataTable.GetValue(0, intLoop)
                        If strCurrency = strMainCurrency Then
                            oApplication.Utilities.Message("Transaction currency already exists ", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            Return False
                        End If
                    End If
                Next
            End If
        Next

        For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            strMainCurrency = oGrid.DataTable.GetValue(0, intRow)
            If strMainCurrency <> "-" And strMainCurrency <> "" Then
                If oGrid.DataTable.GetValue("ActCode", intRow) = "" Then
                    oApplication.Utilities.Message("G/L Account can not be empty : " & strMainCurrency, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If

                If oGrid.DataTable.GetValue(1, intRow) <= 0 Then
                    oApplication.Utilities.Message("Transaction amount should be greater than zero for currency : " & strMainCurrency, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If
                If strpostingdate <> "" Then
                    LCExangeRage = oApplication.Utilities.getExchangeRate(oGrid.DataTable.GetValue(0, intRow), dtPostingdate)
                Else
                    LCExangeRage = oApplication.Utilities.getExchangeRate(oGrid.DataTable.GetValue(0, intRow))
                End If

                If LCExangeRage <= 0 Then
                    oApplication.Utilities.Message("Exchange Rate does not defined for currency : " & strMainCurrency, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    oApplication.SBO_Application.Menus.Item("3333").Activate()
                    Return False
                Else
                    If getPaymentMethod() = True Then
                        LCAmount = oGrid.DataTable.GetValue(1, intRow) * LCExangeRage
                    Else
                        LCAmount = oGrid.DataTable.GetValue(1, intRow) / LCExangeRage
                    End If

                    ' LCAmount = oGrid.DataTable.GetValue(1, intRow) * LCExangeRage
                    oGrid.DataTable.SetValue(2, intRow, LCAmount)
                    oGrid.DataTable.SetValue(3, intRow, LCExangeRage)
                End If
            End If

        Next
        strMainCurrency = ""
        strCurrency = ""
        For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            dblAmount = dblAmount + oGrid.DataTable.GetValue(1, intRow)
            If oGrid.DataTable.GetValue(0, intRow) <> "" Then
                If intRow = 0 Then
                    strMainCurrency = oGrid.DataTable.GetValue(0, intRow)
                End If
                strCurrency = oGrid.DataTable.GetValue(0, intRow)
                If strMainCurrency <> strCurrency Then
                    blnMulticurrency = True
                End If
            End If
        Next

        If dblAmount <= 0 Then
            oApplication.Utilities.Message("Transfer amount should be greater than zeor", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If

        For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            If oGrid.DataTable.GetValue(0, intRow) <> "" Then


                strAccountCurrency = getAcctCurrency(oGrid.DataTable.GetValue("ActCode", intRow))

                If blnMulticurrency = True Then
                    If strBPCurrency <> "##" Then
                        oApplication.Utilities.Message("Busienss Partner does allowed to pay in multicurrency", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        Return False
                    End If
                    If strAccountCurrency <> "##" Then
                        oApplication.Utilities.Message("Account Currency does not match with docuemnt currency : Line No :" & intRow + 1, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        Return False
                    End If
                Else
                    If strBPCurrency <> strCurrency And strBPCurrency <> "##" Then
                        oApplication.Utilities.Message("Transfer Currency differ from BP Currency : Line No :" & intRow + 1, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        Return False
                    End If
                    If strAccountCurrency <> "##" Then
                        If strAccountCurrency <> strBPCurrency And strBPCurrency <> "##" Then
                            oApplication.Utilities.Message("Account Currency does not match with docuemnt currency: Line No :" & intRow + 1, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            Return False
                        End If
                    End If
                End If
            End If
        Next

        oGrid = aForm.Items.Item("6").Specific
        If oGrid.DataTable.Rows.Count < 1 Then
            oApplication.Utilities.Message("No invoices exists for payments..", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        ElseIf oGrid.DataTable.Rows.Count = 1 Then
            Dim intdoc As Integer
            intdoc = oGrid.DataTable.GetValue(0, 0)
            If intdoc <= 0 Then
                oApplication.Utilities.Message("No invoices exists for payments..", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            End If
        End If

        'strSQL = "Select sum(DocTotal-PaidTodate) from OINV where DocTotal > PaidToDate and cardcode ='" & strcardcode & "'"
        'oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        'oTempRec.DoQuery(strSQL)
        'If Math.Round(dblAmount, 2) > Math.Round(oTempRec.Fields.Item(0).Value, 2) Then
        '    If oApplication.SBO_Application.MessageBox("Payment amount is greater than Invoice amount. Do you want to post the excess amount as payment on account?", , "Yes", "No") = 2 Then
        '        Return False
        '    End If
        'End If

       
        Return True
    End Function
#End Region

#Region "Create Sales Order"
    Private Function CreateSalesOrder(ByVal aGrid As SAPbouiCOM.Grid, ByVal aform As SAPbouiCOM.Form) As Boolean
        Dim strCardCode, strItemCode As String
        Dim dtDate As Date
        Dim intCopies As Integer
        Dim spath As String
        Dim sw As StreamWriter
        Dim strCurrency, strRowCurrency, strdate, strDocCurrency, strpostingdate As String
        Dim dbltransferamt, dblLCTransAmount As Double
        Dim dtTransferDate As Date
        Dim intRowCount As Integer = 0
        Dim oPayment As SAPbobsCOM.Payments
        Dim strPaymentChoice As String
        spath = System.Windows.Forms.Application.StartupPath & "\LogDetails.txt"
        If File.Exists(spath) Then
            File.Delete(spath)
        End If
        Try
            dtResult = aform.DataSources.DataTables.Item("dtResult")
            dtResult.Rows.Clear()
            If oApplication.Company.InTransaction() Then
                oApplication.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
            End If
            oComboBox = aform.Items.Item("20").Specific
            strPaymentChoice = oComboBox.Selected.Value
            oGrid1 = aform.Items.Item("10").Specific
            oGrid = aform.Items.Item("6").Specific
            strCardCode = oApplication.Utilities.getEditTextvalue(aform, "7")
            strdate = oApplication.Utilities.getEditTextvalue(aform, "15")
            strpostingdate = oApplication.Utilities.getEditTextvalue(aform, "23")
            If strdate <> "" Then
                dtTransferDate = aform.DataSources.UserDataSources.Item("TransDate").Value '  oApplication.Utilities.GetDateTimeValue(strdate)
            End If
            If strpostingdate <> "" Then
                dtPostingdate = aform.DataSources.UserDataSources.Item("DocDate").Value '  oApplication.Utilities.GetDateTimeValue(strdate)
            End If
            blnErrorLog = False
            '    sw = New StreamWriter(spath, True)
            oApplication.Company.StartTransaction()
            For intCurrency As Integer = 0 To oGrid1.DataTable.Rows.Count - 1
                strCurrency = oGrid1.DataTable.GetValue(0, intCurrency)
                dbltransferamt = oGrid1.DataTable.GetValue(1, intCurrency)
                dbltransferamt = Math.Round(dbltransferamt, 2)
                dblLCTransAmount = oGrid1.DataTable.GetValue(2, intCurrency)
                dblLCTransAmount = Math.Round(dblLCTransAmount, 2)
                intRowCount = 0
                If dbltransferamt > 0 And dblLCTransAmount > 0 Then
                    oPayment = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)
                    oPayment.DocType = SAPbobsCOM.BoRcptTypes.rCustomer
                    oPayment.DocDate = dtPostingdate
                    oPayment.CardCode = strCardCode
                    If strPaymentChoice = "Cash" Then
                        '  oPayment.CashAccount = getAcctCode(oApplication.Utilities.getEditTextvalue(aform, "13"))
                        oPayment.CashAccount = getAcctCode(oGrid1.DataTable.GetValue("ActCode", intCurrency))

                        oPayment.CashSum = dbltransferamt
                    Else
                        ' oPayment.TransferAccount = getAcctCode(oApplication.Utilities.getEditTextvalue(aform, "13"))
                        oPayment.TransferAccount = getAcctCode(oGrid1.DataTable.GetValue("ActCode", intCurrency))
                        oPayment.TransferSum = dbltransferamt
                        oPayment.TransferDate = dtTransferDate
                    End If
                    oPayment.DocCurrency = strCurrency
                    For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
                        strRowCurrency = oGrid.DataTable.GetValue("U_Currency", intRow)
                        If strCurrency = strRowCurrency Then
                            If intRowCount > 0 Then
                                oPayment.Invoices.Add()
                                oPayment.Invoices.SetCurrentLine(intRowCount)
                            End If
                            strDocCurrency = oApplication.Utilities.GetDocCurrency(oGrid.DataTable.GetValue("U_DocEntry", intRow))
                            oPayment.Invoices.DocEntry = oGrid.DataTable.GetValue("U_DocEntry", intRow)
                            Dim dblexchange As Double
                            If strCurrency <> strDocCurrency Then
                                If strpostingdate <> "" Then
                                    dblexchange = oApplication.Utilities.getExchangeRate(strDocCurrency, dtPostingdate)
                                Else
                                    dblexchange = oApplication.Utilities.getExchangeRate(strDocCurrency)
                                End If
                                oPayment.Invoices.SumApplied = oGrid.DataTable.GetValue("U_AppAmt", intRow)
                                oPayment.Invoices.AppliedFC = oGrid.DataTable.GetValue("U_AppAmt", intRow) / dblexchange
                            Else
                                dblexchange = 1
                                oPayment.Invoices.SumApplied = oGrid.DataTable.GetValue("U_AppAmt", intRow)
                            End If
                            intRowCount = intRowCount + 1
                        End If
                    Next
                    If intRowCount > 0 Then
                        If oPayment.Add <> 0 Then
                            sw = New StreamWriter(spath, True)
                            sw.WriteLine("Documents generated are rolled back")
                            sw.WriteLine("Error occured: " & oApplication.Company.GetLastErrorDescription)
                            sw.Flush()
                            sw.Close()
                            blnErrorLog = True
                        Else
                            Dim strDocNum As String
                            oApplication.Company.GetNewObjectCode(strDocNum)
                            oPayment = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments)
                            If oPayment.GetByKey(Convert.ToInt32(strDocNum)) Then
                                dtResult.Rows.Add()
                                dtResult.SetValue("CardCode", dtResult.Rows.Count - 1, strCardCode)
                                dtResult.SetValue("DocEntry", dtResult.Rows.Count - 1, oPayment.DocEntry)
                                dtResult.SetValue("DocNum", dtResult.Rows.Count - 1, oPayment.DocNum)
                                dtResult.SetValue("Currency", dtResult.Rows.Count - 1, oPayment.DocCurrency)
                                dtResult.SetValue("Amount", dtResult.Rows.Count - 1, oPayment.TransferSum)
                                strDocNum = oPayment.DocNum
                            End If
                            If File.Exists(spath) Then
                            End If
                            Dim strMessage As String
                            sw = New StreamWriter(spath, True)
                            strMessage = "Document created successfully : Cardcode : " & strCardCode & "   DocNum  --> " & strDocNum
                            sw.WriteLine(strMessage)
                            sw.Flush()
                            sw.Close()
                        End If
                    End If
                End If
            Next
            If blnErrorLog = True Then
                If oApplication.Company.InTransaction() Then
                    oApplication.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                End If
                Return False
            Else
                If oApplication.Company.InTransaction() Then
                    oApplication.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                End If
            End If
            
            Return True
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            If oApplication.Company.InTransaction() Then
                oApplication.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
            End If
            Return False
        End Try
    End Function
#End Region


#Region "Get Customer Invoice Balance "
    Private Function GetCustomerInvoiceBalance(ByVal aCardCode As String) As Double
        Dim oTempRec As SAPbobsCOM.Recordset
        Dim dblAmount As Double
        aCardCode = oApplication.Utilities.getEditTextvalue(oForm, "7")
        strSQL = "Select sum(DocTotal-PaidTodate) from OINV where DocTotal > PaidToDate and cardcode ='" & aCardCode & "'"
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery(strSQL)
        dblamount = oTempRec.Fields.Item(0).Value
        Return Math.Round((dblAmount), 2)
    End Function
#End Region
#Region "DataBind Result"
    Private Sub DataBindResult(ByVal aform As SAPbouiCOM.Form, ByVal dtTable As SAPbouiCOM.DataTable)
        oGrid = aform.Items.Item("3").Specific
        oGrid.DataTable = dtTable
    End Sub
#End Region

#Region "Menu Event"
    Public Overrides Sub MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Select Case pVal.MenuUID
            Case mnu_FIRST, mnu_LAST, mnu_NEXT, mnu_PREVIOUS
            Case mnu_BatchOrders
                If pVal.BeforeAction = False Then
                    LoadForm()
                End If
            Case mnu_ADD

        End Select
    End Sub
#End Region

#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            Select Case pVal.BeforeAction
                Case True
                    If pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.ItemUID = "3" Then
                        oMode = pVal.FormMode
                        oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                        ' oForm.Items.Item("4").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                        If Validation(oForm) = False Then
                            BubbleEvent = False
                            Exit Sub
                        Else
                            oForm.Items.Item("4").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                        End If


                    End If
                    If (pVal.ItemUID = "4" And pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED) Then
                        If Validation(oForm) = False Then
                            BubbleEvent = False
                            Exit Sub
                        End If
                    End If
                Case False
                    Select Case pVal.EventType
                        Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                            oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                            oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        Case SAPbouiCOM.BoEventTypes.et_COMBO_SELECT
                            oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                            If pVal.ItemUID = "10" And pVal.ColUID = "Currency" Then
                                oGrid = oForm.Items.Item("10").Specific
                                If pVal.Row = oGrid.DataTable.Rows.Count - 1 Then
                                    If oGrid.DataTable.GetValue(0, pVal.Row) <> "-" Then
                                        oGrid.DataTable.Rows.Add()
                                    End If
                                ElseIf oGrid.DataTable.GetValue(0, pVal.Row) = "-" Then
                                    oGrid.DataTable.Rows.Remove(pVal.Row)
                                    Exit Sub
                                End If
                                Dim strPostingdate As String
                                strPostingdate = oApplication.Utilities.getEditTextvalue(oForm, "23")
                                If strPostingdate <> "" Then
                                    dtPostingdate = oForm.DataSources.UserDataSources.Item("DocDate").Value '  oApplication.Utilities.GetDateTimeValue(strdate)
                                    LCExangeRage = oApplication.Utilities.getExchangeRate(oGrid.DataTable.GetValue(0, pVal.Row), dtPostingdate)
                                Else
                                    LCExangeRage = oApplication.Utilities.getExchangeRate(oGrid.DataTable.GetValue(0, pVal.Row))
                                End If

                                If LCExangeRage <= 0 Then
                                    oApplication.Utilities.Message("Exchange rate not defined", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                    oGrid.DataTable.SetValue(3, pVal.Row, LCExangeRage)
                                    oApplication.SBO_Application.Menus.Item("3333").Activate()
                                Else
                                    If getPaymentMethod() = True Then
                                        LCAmount = oGrid.DataTable.GetValue(1, pVal.Row) * LCExangeRage
                                    Else
                                        LCAmount = oGrid.DataTable.GetValue(1, pVal.Row) / LCExangeRage
                                    End If

                                    oGrid.DataTable.SetValue(2, pVal.Row, LCAmount)
                                    oGrid.DataTable.SetValue(3, pVal.Row, LCExangeRage)
                                End If

                            ElseIf pVal.ItemUID = "20" Then
                                oComboBox = oForm.Items.Item("20").Specific
                                If oComboBox.Selected.Value = "Cash" Then
                                    oForm.Items.Item("14").Visible = False
                                    oForm.Items.Item("15").Visible = False
                                Else
                                    oForm.Items.Item("14").Visible = True
                                    oForm.Items.Item("15").Visible = True
                                End If
                            End If
                        Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                            If pVal.ItemUID = "10" And pVal.ColUID = "Amount" And pVal.CharPressed = 9 Then
                                oGrid = oForm.Items.Item("10").Specific
                                LCAmount = 0
                                If oGrid.DataTable.GetValue(0, pVal.Row) <> "-" And oGrid.DataTable.GetValue(1, pVal.Row) > 0 Then
                                    Dim strPostingdate As String
                                    strPostingdate = oApplication.Utilities.getEditTextvalue(oForm, "23")
                                    If strPostingdate <> "" Then
                                        dtPostingdate = oForm.DataSources.UserDataSources.Item("DocDate").Value '  oApplication.Utilities.GetDateTimeValue(strdate)
                                        LCExangeRage = oApplication.Utilities.getExchangeRate(oGrid.DataTable.GetValue(0, pVal.Row), dtPostingdate)
                                    Else
                                        LCExangeRage = oApplication.Utilities.getExchangeRate(oGrid.DataTable.GetValue(0, pVal.Row))
                                    End If
                                    'LCExangeRage = oApplication.Utilities.getExchangeRate(oGrid.DataTable.GetValue(0, pVal.Row))
                                    If LCExangeRage <= 0 Then
                                        oApplication.Utilities.Message("Exchange rate not defined", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                        oApplication.SBO_Application.Menus.Item("3333").Activate()
                                    Else
                                        ' LCAmount = oGrid.DataTable.GetValue(1, pVal.Row) * LCExangeRage
                                        If getPaymentMethod() = True Then
                                            LCAmount = oGrid.DataTable.GetValue(1, pVal.Row) * LCExangeRage
                                        Else
                                            LCAmount = oGrid.DataTable.GetValue(1, pVal.Row) / LCExangeRage
                                        End If

                                        oGrid.DataTable.SetValue(2, pVal.Row, LCAmount)
                                        oGrid.DataTable.SetValue(3, pVal.Row, LCExangeRage)
                                    End If
                                Else
                                    LCAmount = 0
                                    LCExangeRage = 0
                                    oGrid.DataTable.SetValue(1, pVal.Row, LCAmount)
                                    oGrid.DataTable.SetValue(2, pVal.Row, LCAmount)
                                    oGrid.DataTable.SetValue(3, pVal.Row, LCExangeRage)
                                End If
                            End If
                        Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                            Dim oCFLEvento As SAPbouiCOM.IChooseFromListEvent
                            Dim oCFL As SAPbouiCOM.ChooseFromList
                            Dim val1, Currency As String
                            Dim sCHFL_ID, val As String
                            Dim intChoice As Integer
                            Try
                                oCFLEvento = pVal
                                sCHFL_ID = oCFLEvento.ChooseFromListUID
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                oCFL = oForm.ChooseFromLists.Item(sCHFL_ID)
                                If (oCFLEvento.BeforeAction = False) Then
                                    Dim oDataTable As SAPbouiCOM.DataTable
                                    oDataTable = oCFLEvento.SelectedObjects
                                    oGrid = oForm.Items.Item("6").Specific
                                    intChoice = 0
                                    oForm.Freeze(True)
                                    If ((pVal.ItemUID = "7")) Then
                                        val = oDataTable.GetValue("CardCode", 0)
                                        val1 = oDataTable.GetValue("CardName", 0)
                                        Currency = oDataTable.GetValue("Currency", 0)
                                        oEditText = oForm.Items.Item("9").Specific
                                        oEditText.Value = val1
                                        GetDocuments(val, oForm)
                                        oStatictext = oForm.Items.Item("18").Specific
                                        If Currency = "##" Then
                                            Currency = "All"
                                        End If
                                        oStatictext.Caption = "BP Currency : " & Currency
                                        oStatictext = oForm.Items.Item("26").Specific
                                        Dim dblAmt As Double
                                        dblAmt = oDataTable.GetValue("Balance", 0)
                                        oStatictext.Caption = "Invoice Balance (LC) : " & dblAmt.ToString
                                        oEditText = oForm.Items.Item("7").Specific
                                        oEditText.Value = val
                                    ElseIf pVal.ItemUID = "13" Then
                                        val = oDataTable.GetValue("FormatCode", 0)
                                        val1 = oDataTable.GetValue("AcctName", 0)
                                        oEditText = oForm.Items.Item("13").Specific
                                        oStatictext = oForm.Items.Item("21").Specific
                                        oStatictext.Caption = val1
                                        val1 = oDataTable.GetValue("AcctCode", 0)
                                        oStatictext = oForm.Items.Item("211").Specific
                                        oStatictext.Caption = val1
                                        oEditText.Value = val
                                    ElseIf pVal.ItemUID = "10" And pVal.ColUID = "ActCode" Then
                                        val = oDataTable.GetValue("FormatCode", 0)
                                        val1 = oDataTable.GetValue("AcctName", 0)
                                        oGrid = oForm.Items.Item("10").Specific
                                        oGrid.DataTable.SetValue("ActCode", pVal.Row, val)
                                        oGrid.DataTable.SetValue("ActName", pVal.Row, val1)
                                    End If
                                    oForm.Freeze(False)
                                End If
                            Catch ex As Exception
                                oForm.Freeze(False)
                            End Try
                        Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                            oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                            oGrid = oForm.Items.Item("6").Specific
                            Try

                                Select Case pVal.ItemUID
                                    Case "4"
                                        oForm.Freeze(True)
                                        AddinSequence(oForm)
                                        oForm.Freeze(False)
                                    Case "5"
                                        oForm.Freeze(True)
                                        GetDocuments(oApplication.Utilities.getEditTextvalue(oForm, "7"), oForm)
                                        ClearCurrencyMatrix(oForm)
                                        oForm.Freeze(False)
                                    Case "3"
                                        If 1 = 1 Then
                                            oForm.Freeze(True)
                                            If oForm.DataSources.DataTables.Count < 0 Then
                                                oForm.DataSources.DataTables.Add("dtResult")
                                            End If
                                            If oApplication.SBO_Application.MessageBox("Do you want to create the Incoming payment?", , "Yes", "No") = 2 Then
                                                oForm.Freeze(False)
                                                Exit Sub
                                            End If
                                            Dim dblAmount As Double = 0
                                            Dim strCardCode As String
                                            oGrid = oForm.Items.Item("10").Specific
                                            For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
                                                dblAmount = dblAmount + oGrid.DataTable.GetValue(2, intRow)
                                            Next
                                            Dim oTempRec As SAPbobsCOM.Recordset
                                            strCardCode = oApplication.Utilities.getEditTextvalue(oForm, "7")
                                            strSQL = "Select sum(DocTotal-PaidTodate) from OINV where DocTotal > PaidToDate and cardcode ='" & strCardCode & "'"
                                            oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                            oTempRec.DoQuery(strSQL)
                                            If Math.Round(dblAmount, 2) > Math.Round(oTempRec.Fields.Item(0).Value, 2) Then
                                                If oApplication.SBO_Application.MessageBox("Payment amount is greater than Invoice amount. Do you want to post the excess amount as payment on account?", , "Yes", "No") = 2 Then
                                                    oForm.Freeze(False)
                                                    Exit Sub
                                                End If
                                            End If
                                            oForm.Freeze(False)
                                            If CreateSalesOrder(oGrid, oForm) = True Then
                                                If blnErrorLog = False Then
                                                    Dim spath As String
                                                    Dim x As System.Diagnostics.ProcessStartInfo
                                                    oApplication.Utilities.Message("Incoming Payments created successfully", SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                                                    oForm.Close()
                                                    x = New System.Diagnostics.ProcessStartInfo
                                                    x.UseShellExecute = True
                                                    spath = System.Windows.Forms.Application.StartupPath & "/LogDetails.txt"
                                                    x.FileName = spath
                                                    System.Diagnostics.Process.Start(x)
                                                    x = Nothing
                                                Else
                                                    Dim spath As String
                                                    Dim x As System.Diagnostics.ProcessStartInfo
                                                    oApplication.Utilities.Message("Errors in creating Payments ", SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                                                    oForm.Close()
                                                    x = New System.Diagnostics.ProcessStartInfo
                                                    x.UseShellExecute = True
                                                    spath = System.Windows.Forms.Application.StartupPath & "/LogDetails.txt"
                                                    x.FileName = spath
                                                    System.Diagnostics.Process.Start(x)
                                                    x = Nothing
                                                End If
                                            Else
                                                Dim spath As String
                                                Dim x As System.Diagnostics.ProcessStartInfo
                                                oApplication.Utilities.Message("Errors in creating Payments ", SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                                                oForm.Close()
                                                x = New System.Diagnostics.ProcessStartInfo
                                                x.UseShellExecute = True
                                                spath = System.Windows.Forms.Application.StartupPath & "/LogDetails.txt"
                                                x.FileName = spath
                                                System.Diagnostics.Process.Start(x)
                                                x = Nothing
                                            End If
                                        End If
                                End Select
                            Catch ex As Exception
                                oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            End Try
                    End Select
            End Select
        Catch ex As Exception
            oForm = oApplication.SBO_Application.Forms.ActiveForm()
            oForm.Freeze(False)
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#End Region


End Class
