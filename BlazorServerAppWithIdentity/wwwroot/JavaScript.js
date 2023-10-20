function showNotification(title, options) {
    if ("Notification" in window) {
        Notification.requestPermission().then(function (permission) {
            if (permission === "granted") {
                new Notification(title, options);
            }
        });
    }
}