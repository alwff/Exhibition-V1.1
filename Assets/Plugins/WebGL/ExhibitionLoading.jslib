mergeInto(LibraryManager.library, {
    ExhibitionLoadProgress: function (completed, total) {
        window.exhibitionLoadState = {
            completed: completed,
            total: total
        };

        if (typeof window.onExhibitionLoadProgress === "function") {
            window.onExhibitionLoadProgress(completed, total);
        }
    },

    ExhibitionLoadReady: function () {
        window.exhibitionContentReady = true;

        if (typeof window.onExhibitionLoadReady === "function") {
            window.onExhibitionLoadReady();
        }
    },

    ExhibitionLoadFailed: function () {
        window.exhibitionLoadFailed = true;

        if (typeof window.onExhibitionLoadFailed === "function") {
            window.onExhibitionLoadFailed();
        }
    }
});