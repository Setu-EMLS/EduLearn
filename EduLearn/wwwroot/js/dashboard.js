// Shared behaviour for the Admin / Teacher / Student dashboards
(function ($) {
    // Mobile sidebar
    $(document).on('click', '[data-dash-toggle]', function () {
        $('#dashSidebar').toggleClass('open');
        $('.dash-backdrop').toggleClass('show');
    });

    // Tables with class "datatable" get search / paging / sorting
    $('table.datatable').each(function () {
        var $t = $(this);
        $t.DataTable({
            pageLength: $t.data('page-length') || 10,
            order: [],
            columnDefs: [{ targets: 'no-sort', orderable: false }],
            language: { search: '', searchPlaceholder: 'Search...' }
        });
    });

    // <form data-confirm="Are you sure?"> asks before submitting
    $(document).on('submit', 'form[data-confirm]', function (e) {
        if (!window.confirm($(this).data('confirm'))) {
            e.preventDefault();
        }
    });

    // Success alerts fade out on their own
    setTimeout(function () {
        $('.dash-content > .alert-success').fadeOut(400);
    }, 5000);
})(jQuery);
