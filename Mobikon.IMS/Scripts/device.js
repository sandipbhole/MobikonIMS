$(document).ready(function () {
    $("dcDate").blur(function () {
        $("#deliveryDate").val($("dcDate").val());       
    });
});

$(document).ready(function () {    
    $("dcDate").focusout(function () {
        $("#deliveryDate").val($("dcDate").val());
    });
});


$(document).ready(function () {
    $("#clientName").change(function () {
        if ($("#clientName").val() != "" && $("#clientName").val() != "Select client/account name")
        {
            $("#clientID").val(0);
            //$("#clientAddress").val("");           
            //$("#clientCityName").val("");         
            //$("#clientCountryName").val("");
         
            $("#outletID").val(0);
            $("#outletName").empty();           
            $("#outletName").append("<option> Select outlet </option>");

            $("#cityID").val(0);
            //$("#cityName").val("");
            $("#countryID").val(0);
            //$("#countryName").val("");

            var OutletOptions = {};
            OutletOptions.url = "/MobikonIMS/Outlet/GetClientwiseOutletName";
            OutletOptions.type = "POST";
            OutletOptions.data = JSON.stringify({ clientName: $("#clientName").val() });
            OutletOptions.datatype = "json";
            OutletOptions.contentType = "application/json";
            OutletOptions.success = function (OutletList) {
                for (var i = 0; i < OutletList.length; i++)
                {
                    $("#clientID").val(OutletList[i].clientID);
                    //$("#clientAddress").val(OutletList[i].clientAddress);                   
                    //$("#clientCityName").val(OutletList[i].clientCity);                   
                    //$("#clientCountryName").val(OutletList[i].clientCountry);

                    $("#outletID").val(OutletList[i].outletID);
                    $("#outletName").append("<option>" + OutletList[i].outletName + "</option>");
                }
            };
            OutletOptions.error = function () { alert("Error in Getting Client Details!!"); };
            $.ajax(OutletOptions);
        }       
        else
        {
            $("#clientID").val(0);
            $("#clientAddress").val("");
            $("#clientCityName").val("");
            $("#clientCountryName").val("");

            $("#outletID").val(0);
            $("#outletName").empty();
            $("#outletName").append("<option> Select outlet </option>");

            $("#cityID").val(0);
            $("#cityName").val("");
            $("#countryID").val(0);
            $("#countryName").val("");
        }
    });
});

$(document).ready(function () {
    $("#productSerial").change(function () {
        if ($("#productSerial").val() != "Select device serial") {
            var ProductOptions = {};
            ProductOptions.url = "/MobikonIMS/Device/GetProductSerialwiseDevice";
            ProductOptions.type = "POST";
            ProductOptions.data = JSON.stringify({ productSerial: $("#productSerial").val() });
            ProductOptions.datatype = "json";
            ProductOptions.contentType = "application/json";
            ProductOptions.success = function (ProductList) {
                $("#deviceID").val(ProductList.deviceID);
                $("#deviceDetails").val(ProductList.deviceDetails);
                $("#companyOwner").val(ProductList.companyOwner);
            };
            ProductOptions.error = function () { alert("Error in Getting Device Details!!"); };
            $.ajax(ProductOptions);
        }
    });
});

$(document).ready(function () {
    $("#outletName").change(function () {
        if ($("#outletName").val() != "Select outlet")
        {
            $("#outletID").val(0);
            //$("#address").val("");
            $("#countryID").val(0);
            //$("#countryName").val("");
            $("#cityID").val(0);
            //$("#cityName").val("");

            var OutletOptions = {};
            OutletOptions.url = "/MobikonIMS/Outlet/GetOutletnamewiseOutlet";
            OutletOptions.type = "POST";
            OutletOptions.data = JSON.stringify({ outletName: $("#outletName").val() });
            OutletOptions.datatype = "json";
            OutletOptions.contentType = "application/json";
            OutletOptions.success = function (OutletList) {
                $("#outletID").val(OutletList.outletID);
                //$("#address").val(OutletList.address);
                $("#countryID").val(OutletList.countryID);
                //$("#countryName").val(OutletList.countryName);
                $("#cityID").val(OutletList.cityID);
                //$("#cityName").val(OutletList.cityName);

            };
            OutletOptions.error = function (e) {
                alert("Error in Getting Device Details!!");
            };
            $.ajax(OutletOptions);
        }
        else {
            $("#outletID").val(0);
            $("#address").val("");
            $("#countryID").val(0);
            $("#countryName").val("");
            $("#cityName").val("");
            $("#cityID").val(0);
        }
    });
});

$(document).ready(function () {
    $(function () { // will trigger when the document is ready
        $('.datepicker').datepicker({
            autoclose: true,
            format: 'dd/mm/yyyy',
            fixFocusIE: false,
            onClose: function (text, inst)
            {
                this.fixFocusIE = true;
                $("#deliveryDate").val(text);
                $(this).focus();
            },
            beforeShow: function(input, inst) {
                var result = $.browser.msie ? !this.fixFocusIE : true;
                this.fixFocusIE = false;
                return result;
            },
            //todayBtn: true,
            todayHighlight: true,
            showOn: "button",
            buttonImage: "images/calendar.gif",
            buttonImageOnly: true,
            buttonText: "Select date"
        }); //Initialise any date pickers
    });
});

$(document).ready(function () {
    $("#outletName").change(function () {
        if ($("#status").val() != "Select status")
        {
            $("#statusID").val(0);

            var StatusOptions = {};
            StatusOptions.url = "/MobikonIMS/Outlet/GetStatusnamewiseStatus";
            StatusOptions.type = "POST";
            StatusOptions.data = JSON.stringify({ statusName: $("#status").val() });
            StatusOptions.datatype = "json";
            StatusOptions.contentType = "application/json";
            StatusOptions.success = function (StatusList)
            {
                $("#statusID").val(StatusList.statusID);
            };
            StatusOptions.error = function (e) {
                alert("Error in Getting Status Details!!");
            };
            $.ajax(StatusOptions);
        }
        else {
            $("#statusID").val(0);          
        }
    });
});
