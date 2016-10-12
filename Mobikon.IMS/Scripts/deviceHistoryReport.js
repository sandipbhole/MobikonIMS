$(document).ready(function () {
    $("#productSerial").change(function () {
        if ($("#productSerial").val() != "All") {
            var DeviceOptions = {};
            DeviceOptions.url = "/MobikonIMS/Device/GetProductSerialwiseDevice";
            DeviceOptions.type = "POST";
            DeviceOptions.data = JSON.stringify({ productSerial: $("#productSerial").val() });
            DeviceOptions.datatype = "json";
            DeviceOptions.contentType = "application/json";
            DeviceOptions.success = function (DeviceList)
            {
                $("#deviceDetails").prop("readonly", true);
                $("#deviceID").val(DeviceList.deviceID);
                $("#deviceDetails").val(DeviceList.deviceDetails);
                $("#statusID").val(DeviceList.statusID);
                $("#status").append("<option>" + DeviceList.status + "</option>");
            };
            DeviceOptions.error = function () { alert("Error in Getting Device Details!!"); };
            $.ajax(DeviceOptions);
        }
        else if ($("#productSerial").val() == "All")
        {
            $("#deviceDetails").prop("readonly", false);
            $("#deviceID").val(0);
            $("#deviceDetails").val("All");
            $("#statusID").val(0);
            $("#status").append("<option>All</option>");
        }
    });
});


$(document).ready(function () {
    $("#clientName").change(function () {
        if ($("#clientName").val() != "" && $("#clientName").val() != "All") {
            $("#outletName").empty();
            $("#outletID").val(0);
            $("#outletName").append("<option>All</option>");

            var OutletOptions = {};
            OutletOptions.url = "/MobikonIMS/Outlet/GetClientwiseOutletName";
            OutletOptions.type = "POST";
            OutletOptions.data = JSON.stringify({ clientName: $("#clientName").val() });
            OutletOptions.datatype = "json";
            OutletOptions.contentType = "application/json";
            OutletOptions.success = function (OutletList) {
                for (var i = 0; i < OutletList.length; i++) {
                    $("#outletID").val(OutletList[i].outletID);
                    $("#outletName").append("<option>" + OutletList[i].outletName + "</option>");                   
                }
            };
            OutletOptions.error = function () { alert("Error in Getting Client Details!!"); };
            $.ajax(OutletOptions);
        }
        else {
            $("#outletName").empty();
            $("#outletID").val(0);
            $("#outletName").append("<option>All</option>");           
        }
    });
});

$(document).ready(function () {
    $(function () { // will trigger when the document is ready
        $('.datepicker').datepicker({
            autoclose: true,
            format: 'dd/mm/yyyy',
            todayBtn: true,
            todayHighlight: true,
            showOn: "button",
            buttonImage: "images/calendar.gif",
            buttonImageOnly: true,
            buttonText: "Select date"


        }); //Initialise any date pickers
    });
});