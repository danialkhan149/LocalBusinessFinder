window.lbfMaps = (function () {
    const maps = {};

    function init(mapId, lat, lng, zoom) {
        if (maps[mapId]) {
            maps[mapId].map.remove();
        }
        const el = document.getElementById(mapId);
        if (!el) return;
        const map = L.map(mapId).setView([lat, lng], zoom);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap'
        }).addTo(map);
        maps[mapId] = { map, markers: {}, route: null };
        setTimeout(() => map.invalidateSize(), 200);
    }

    function setCenter(mapId, lat, lng, zoom) {
        const m = maps[mapId];
        if (!m) return;
        m.map.setView([lat, lng], zoom);
    }

    function updateMarker(mapId, markerId, lat, lng, label, color) {
        const m = maps[mapId];
        if (!m) return;
        if (m.markers[markerId]) {
            m.markers[markerId].setLatLng([lat, lng]);
        } else {
            const icon = L.divIcon({
                className: 'lbf-marker',
                html: `<div style="background:${color};width:14px;height:14px;border-radius:50%;border:2px solid #fff;box-shadow:0 1px 4px rgba(0,0,0,.4)"></div>`,
                iconSize: [14, 14],
                iconAnchor: [7, 7]
            });
            m.markers[markerId] = L.marker([lat, lng], { icon }).addTo(m.map).bindPopup(label);
        }
        m.map.panTo([lat, lng]);
    }

    function removeMarker(mapId, markerId) {
        const m = maps[mapId];
        if (!m || !m.markers[markerId]) return;
        m.map.removeLayer(m.markers[markerId]);
        delete m.markers[markerId];
    }

    function drawRoute(mapId, points) {
        const m = maps[mapId];
        if (!m || !points || points.length < 2) return;
        if (m.route) m.map.removeLayer(m.route);
        const latlngs = points.map(p => [p[0], p[1]]);
        m.route = L.polyline(latlngs, { color: '#198754', weight: 4, opacity: 0.8 }).addTo(m.map);
        m.map.fitBounds(m.route.getBounds(), { padding: [40, 40] });
    }

    function destroy(mapId) {
        const m = maps[mapId];
        if (m) {
            m.map.remove();
            delete maps[mapId];
        }
    }

    return { init, setCenter, updateMarker, removeMarker, drawRoute, destroy };
})();

window.lbfSignalR = {
    chatConnection: null,
    trackingConnection: null,

    async connectChat(requestId, dotNetRef) {
        if (this.chatConnection) await this.chatConnection.stop();
        this.chatConnection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/chat')
            .withAutomaticReconnect()
            .build();
        this.chatConnection.on('ReceiveMessage', (msg) => dotNetRef.invokeMethodAsync('OnChatMessage', msg));
        await this.chatConnection.start();
        await this.chatConnection.invoke('JoinRequest', requestId);
    },

    async sendChat(requestId, content, isOffer, offerAmount) {
        if (!this.chatConnection) return;
        await this.chatConnection.invoke('SendMessage', requestId, content, isOffer, offerAmount);
    },

    async connectTracking(requestId, dotNetRef) {
        if (this.trackingConnection) await this.trackingConnection.stop();
        this.trackingConnection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/tracking')
            .withAutomaticReconnect()
            .build();
        this.trackingConnection.on('LocationUpdated', (loc) => dotNetRef.invokeMethodAsync('OnLocationUpdate', loc));
        await this.trackingConnection.start();
        await this.trackingConnection.invoke('JoinTracking', requestId);
    },

    async sendLocation(requestId, lat, lng) {
        if (!this.trackingConnection) return;
        await this.trackingConnection.invoke('UpdateLocation', requestId, lat, lng);
    },

    async disconnectAll() {
        if (this.chatConnection) { await this.chatConnection.stop(); this.chatConnection = null; }
        if (this.trackingConnection) { await this.trackingConnection.stop(); this.trackingConnection = null; }
    },

    async getCurrentPosition() {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject('Geolocation not supported');
                return;
            }
            navigator.geolocation.getCurrentPosition(
                (pos) => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
                (err) => reject(err.message),
                { enableHighAccuracy: true, timeout: 15000 }
            );
        });
    },

    watchPosition(dotNetRef) {
        if (!navigator.geolocation) return null;
        return navigator.geolocation.watchPosition(
            (pos) => dotNetRef.invokeMethodAsync('OnGpsUpdate', pos.coords.latitude, pos.coords.longitude),
            () => { },
            { enableHighAccuracy: true, maximumAge: 5000, timeout: 15000 }
        );
    },

    clearWatch(watchId) {
        if (watchId != null) navigator.geolocation.clearWatch(watchId);
    }
};
