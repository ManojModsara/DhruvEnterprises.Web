
import('https://www.gstatic.com/firebasejs/7.13.2/firebase-app.js');
import('https://www.gstatic.com/firebasejs/7.6.1/firebase-messaging.js');

var firebaseConfig = {
    apiKey: "AIzaSyCrdNUHTGS9LQa74lrN5nOoPIDz_NXoEVE",
    authDomain: "testing-acdda.firebaseapp.com",
    databaseURL: "https://testing-acdda.firebaseio.com",
    projectId: "testing-acdda",
    storageBucket: "testing-acdda.appspot.com",
    messagingSenderId: "358518370528",
    appId: "1:358518370528:web:b3d33c70f0fa1f6dbea085",
    measurementId: "G-ZFFC656P91"
};
// Initialize Firebase
firebase.initializeApp(firebaseConfig);
firebase.analytics();
const messaging = firebase.messaging();
messaging.setBackgroundMessageHandler(function (payload) {
    console.log('[firebase-messaging-sw.js] Received background message ', payload);
    // Customize notification here
    const notificationTitle = payload.data.title;
    const notificationOptions = {
        body: payload.data.body,
        icon: 'http://localhost/gcm-push/img/icon.png',
        image: 'http://localhost/gcm-push/img/d.png'
    };

    return self.registration.showNotification(notificationTitle,
        notificationOptions);
});

messaging.usePublicVapidKey("BG7mFnkIRiIDwyYsRdvntAWfaMr1bTX2mefCtdVTpWs4QrlsRwqxN4rvUQA2DCqdkVnFZPaPIrxOltu5yx_20PU");
function isWebPushSupported() { return ('serviceWorker' in navigator && 'PushManager' in window) }
function askNotificationPermission(cb) {
    Notification.requestPermission().then((permission) => {
        if (Notification.permission == 'granted') {

            messaging.getToken().then((currentToken) => {
                if (currentToken) {
                    updateWebPushToken(currentToken);
                } else {
                    // Show permission request.
                    console.log('No Instance ID token available. Request permission to generate one.');
                    // Show permission UI.
                    setTokenSentToServer(false);
                }
            }).catch((err) => {
                console.log('An error occurred while retrieving token. ', err);
                showToken('Error retrieving Instance ID token. ', err);
                setTokenSentToServer(false);
            });
            messaging.onTokenRefresh(() => {
                messaging.getToken().then((refreshedToken) => {
                    console.log('Token refreshed.');
                    // Indicate that the new Instance ID token has not yet been sent to the
                    // app server.
                    setTokenSentToServer(false);
                    // Send Instance ID token to app server.
                    updateWebPushToken(refreshedToken);
                    // ...
                }).catch((err) => {
                    console.log('Unable to retrieve refreshed token ', err);
                    showToken('Unable to retrieve refreshed token ', err);
                });
            });
            cb();
        }

    });
}
function getNotificationPermissionStatus() {
    return (Notification.permission);
}
function updateWebPushToken(token) {
    $.post("ajax/update-webpush-token.php", { "token": token }, function (data) {
        console.log("webpush token activated");
    });
}

