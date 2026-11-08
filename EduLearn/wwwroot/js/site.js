<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.7.1/jquery.min.js" integrity="sha512-v2CJ7UaYy4JwqLDIrZUI/4hqeoQieOmAZNXBeQyjo21dadnwR+8ZaIJVT8EE2iyI61OV8e6M8PP2/4hpQINQ/g==" crossorigin="anonymous" referrerpolicy="no-referrer"></script>


/*======================= Nav Show to  Hide =======================*/
$('.navbar-collapse a').click(function () {
	$("#navbarNav").collapse('hide');
});


/*======================= Password Hide Show =======================*/
$(".openEye").hide();
$(".closeEye").click(() => {
	$("#Password").attr('type', 'text');
	$(".closeEye").hide();
	$(".openEye").show();
});

$(".openEye").click(() => {
	$("#Password").attr('type', 'password');
	$(".closeEye").show();
	$(".openEye").hide();
});


/*======================= Model Open Hide =======================*/
$(".commonCloseBtn ").click(() => {
	document.getElementById("InformationFrm").reset();
	Update.hide();
	Submit.show();
	$(".commonModal").hide();
});

$(".commonaddBtn ").click(() => {
	document.getElementById("InformationFrm").reset();
	$(".commonModal").show();
	updateTitle.hide();
	addTitle.show();
});