(function ($) {
	'use strict';
	function AdminUserIndex() {
		var $this = this;
		function GetChartData(n, o, f = "", e = "") {
			$.ajax({
				url: "/Home/GetChartOperatorWise?n=" + n + "&o=" + o + "&f=" + f + "&e=" + e,
				type: "Get",
				dataType: "json",
				contentType: "application/json; charset=utf-8",
				success: function (data) {
					var d = JSON.parse(JSON.stringify(data));
					var s = d.data;
					BarChart(s, d.data2,n,f);
				}
			});
		}
		function BarChart(success, failed, n,f) {
			if (n == "" || n == undefined) {
				n = 30;
			}
			var date;
			var month;
			var year;

			debugger;
			if (f == "" || f == undefined) {
				f = $('#txtFromDate').val();
				f = f.split("-");
				year = f[0];
				month = f[1];
			
				f = f[2].split(" ");
				date = f[0];

				f = f[1].split(":");
				if (f[0] == "00") {
					f = 1;
				}
				else {
					f = f[0];
				}
			}
			else {
				f = $('#txtFromDate').val();
				f = f.split("-");
				year = f[0];
				month = f[1];
				f = f[2].split(" ");
				date = f[0];
				
				f = f[1].split(":");
				if (f[0] == "00") {
					f = 1;
				}
				else {
					f = f[0];
				}
			}
			console.log(success);
			console.log(failed);

			var dataPoints = [];
			var success = JSON.parse(success);
			var failed = JSON.parse(failed);
			var x, y = 1;
			for (var i = 1; i < success.length; i++) {
				dataPoints.push({ x: new Date(year, month, date, f, n * i), y: success[i].y });
			}
			var dataPoints2 = [];
			var x, y = 1;
			for (var i = 1; i < failed.length; i++) {
				dataPoints2.push({ x: new Date(year, month, date, f, n * i), y: failed[i].y });
			}
			console.log(dataPoints);
			console.log(dataPoints2);
			console.log(dataPoints);
			var options = {
				exportEnabled: true,
				animationEnabled: true,
				title: {
					text: "Operator Success VS Failed"
				},
				subtitles: [{
					text: "Click Operator to Hide or Unhide Data Series"
				}],
				axisX: {
					title: "States",
					xValueType: "Datetime",
					//valueFormatString: "hh:mm tt",
					interval: 30,
					////intervalType: "datetime",
					////xValueFormatString: success
					valueFormatString: "hh:mm TT"
					//minimum: new Date(2021, 07, 28)
				},
				axisY: {
					title: "Operator SUCCESS",
					titleFontColor: "#C0504E",
					lineColor: "#4F81BC",
					labelFontColor: "#C0504E",
					tickColor: "#C0504E"
				},
				axisY2: {
					title: "Operator FAILED",
					titleFontColor: "#4F81BC",
					lineColor: "#C0504E",
					labelFontColor: "#4F81BC",
					tickColor: "#4F81BC"
				},
				toolTip: {
					shared: true
					//contentFormatter: function (e) {
					//	return CanvasJS.formatDate(e.entries[0].dataPoint.y, "HH:mm:ss") + ": " + e.entries[0].dataPoint.x;
					//}
				},
				legend: {
					cursor: "pointer",
					itemclick: toggleDataSeries
				},
				data: [{
					type: "spline",
					name: "Success",
					//xValueType: "dateTime",
					//xvalueFormatString: "dd/mm/yyyy hh:mm tt",
					showInLegend: true,
					dataPoints: dataPoints
				},
				{
					type: "spline",
					name: "Failed",
					axisYType: "secondary",
					showInLegend: true,
					dataPoints: dataPoints2
				}
				]
			};
			console.log(options);

			var chart = new CanvasJS.Chart("chartContainer", options);
			chart.render();
			function toggleDataSeries(e) {
				if (typeof (e.dataSeries.visible) === "undefined" || e.dataSeries.visible) {
					e.dataSeries.visible = false;
				} else {
					e.dataSeries.visible = true;
				}
				e.chart.render();
			}
			chart.render();

		}
		function initializeModalWithForm() {
			$('.select2').select2({
				placeholder: "--select--",
				allowClear: true
			});
			$('#btnSearch').on('click', function () {

				var Isa = $('#Isa').val();
				var Sdate = $('#txtFromDate').val();
				var txtContactNo = $('#txtContactNo').val();
				if (txtContactNo == "" || txtContactNo == undefined) {
					txtContactNo = "30";
				}
				
				
				var Edate = $('#txtToDate').val();
				var Opid = $('select#ddOperator :Selected').val();
				GetChartData(txtContactNo, Opid, Sdate, Edate);
			});
			

		}

		$this.init = function () {
			GetChartData();
			initializeModalWithForm();
		};
	}
	$(function () {
		var self = new AdminUserIndex();
		self.init();
	});

}(jQuery));