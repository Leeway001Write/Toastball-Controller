package util

import "net"

func GetWifiIP() string {
	var ipString = ""

	ifaces, _ := net.Interfaces()

	for _, iface := range ifaces {
		if (iface.Name == "Wi-Fi") {
			addrs, _ := iface.Addrs()
			for _, addr := range addrs {
				ip, _ := addr.(*net.IPNet)
				if (ip.IP.To4() != nil) {
					ipString = ip.IP.String() + ":8080";
				}
			}
		}
	}

	return ipString
}