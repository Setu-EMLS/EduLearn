// Shared behaviour for the Admin / Teacher / Student dashboards
(function ($) {
    // Mobile sidebar
    $(document).on('click', '[data-dash-toggle]', function () {
        $('#dashSidebar').toggleClass('open');
        $('.dash-backdrop').toggleClass('show');
    });

    // The labels DataTables writes itself ("Show 10 entries", "Search...", paging) in the page's language
    var tableLanguage = document.documentElement.lang === 'bn'
        ? {
            search: '', searchPlaceholder: 'খুঁজুন...',
            lengthMenu: '_MENU_টি করে দেখান',
            info: '_TOTAL_টির মধ্যে _START_ থেকে _END_ পর্যন্ত',
            infoEmpty: 'দেখানোর মতো কিছু নেই',
            infoFiltered: '(মোট _MAX_টি থেকে ছাঁটাই করা)',
            zeroRecords: 'কোনো মিল পাওয়া যায়নি',
            emptyTable: 'কোনো তথ্য নেই',
            paginate: { first: 'প্রথম', last: 'শেষ', next: 'পরবর্তী', previous: 'আগের' }
        }
        : { search: '', searchPlaceholder: 'Search...' };

    // Tables with class "datatable" get search / paging / sorting
    $('table.datatable').each(function () {
        var $t = $(this);
        $t.DataTable({
            pageLength: $t.data('page-length') || 10,
            order: [],
            columnDefs: [{ targets: 'no-sort', orderable: false }],
            language: tableLanguage
        });
    });

    // <form data-confirm="Are you sure?"> asks before submitting
    $(document).on('submit', 'form[data-confirm]', function (e) {
        if (!window.confirm($(this).data('confirm'))) {
            e.preventDefault();
        }
    });

    // Batch drop-downs list only the batches of the course chosen in their course drop-down,
    // and hide themselves while that course has no batches
    $('select[data-batch-for]').each(function () {
        var batch = this;
        var course = document.getElementById(batch.getAttribute('data-batch-for'));
        if (!course) return;
        var wrap = $(batch).closest('[data-batch-wrap]');
        function sync() {
            var any = false;
            $(batch).find('option[data-course]').each(function () {
                var match = this.getAttribute('data-course') === course.value;
                this.hidden = !match;
                this.disabled = !match;
                if (match) any = true;
                if (!match && this.selected) batch.value = '';
            });
            wrap.toggleClass('d-none', !any);
            if (batch.hasAttribute('data-batch-required')) batch.required = any;
        }
        $(course).on('change', sync);
        sync();
    });

    // Batch filters above lists apply as soon as a batch is picked
    $(document).on('change', '.batch-filter select', function () {
        this.form.submit();
    });

    // Success alerts fade out on their own
    setTimeout(function () {
        $('.dash-content > .alert-success').fadeOut(400);
    }, 5000);
})(jQuery);
