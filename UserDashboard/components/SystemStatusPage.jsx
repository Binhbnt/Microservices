import React, { useEffect, useState, useContext } from "react";
import axios from "axios";
import { Server } from "lucide-react";
import { useSession } from "/src/hooks/useSession";
import { PageTitleContext } from "/components/PageTitleContext";
import { HubConnectionBuilder } from "@microsoft/signalr";
import ServiceStatusRow from "/components/ServiceStatusRow";
import StatusFooter from "/components/StatusFooter";
import { API_GATEWAY_URL } from "/src/config";
import SystemStatusPageSkeleton from "/components/SystemStatusPageSkeleton";

const SystemStatusPage = () => {
  const { setPageTitle, setPageIcon } = useContext(PageTitleContext);
  const { getAuthHeaders } = useSession();
  const [serviceLogs, setServiceLogs] = useState({});
  const [loading, setLoading] = useState(true);
  const [lastUpdated, setLastUpdated] = useState(null);

  useEffect(() => {
    setPageTitle("Tình trạng hệ thống");
    setPageIcon('fa-solid fa-server');
  }, [setPageTitle, setPageIcon]);

  // ✅ Thiết lập kết nối SignalR từ API Gateway
  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${API_GATEWAY_URL}/hubs/health`)
      .withAutomaticReconnect()
      .build();

    connection.start().catch(err => console.error("Lỗi kết nối SignalR:", err));

    connection.on("HealthLogUpdated", (log) => {
      setServiceLogs(prevLogs => {
        const updatedLogs = { ...prevLogs };
        const serviceName = log.serviceName;

        if (!updatedLogs[serviceName]) {
          updatedLogs[serviceName] = [];
        }

        updatedLogs[serviceName] = [...updatedLogs[serviceName], log].slice(-60);
        return updatedLogs;
      });
      setLastUpdated(new Date());
    });

    return () => { connection.stop(); };
  }, []);

  // ✅ Tải dữ liệu log ban đầu từ API Gateway
  useEffect(() => {
    const fetchLogs = async () => {
      try {
        const res = await axios.get(`${API_GATEWAY_URL}/api/healthcheck/logs`, {
          headers: getAuthHeaders(),
        });
        setServiceLogs(res.data);
        setLastUpdated(new Date());
      } catch (err) {
        console.error("Lỗi khi tải log:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchLogs();
  }, [getAuthHeaders]);

  return (
    <div style={{ padding: '1.5rem' }}>
      {loading ? (
        <SystemStatusPageSkeleton rows={4} />
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {Object.keys(serviceLogs).length > 0 ? (
            Object.entries(serviceLogs).map(([serviceName, logs]) => (
              <ServiceStatusRow
                key={serviceName}
                serviceName={serviceName}
                logs={logs}
              />
            ))
          ) : (
            <div style={{ color: '#9ca3af' }}>Không có dữ liệu trạng thái để hiển thị.</div>
          )}
        </div>
      )}
      {!loading && <StatusFooter lastUpdated={lastUpdated} />}
    </div>
  );
};

export default SystemStatusPage;
