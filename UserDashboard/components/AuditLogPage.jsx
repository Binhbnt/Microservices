import React, { useState, useEffect, useCallback, useContext, useMemo } from 'react';
import { PageTitleContext } from '/components/PageTitleContext';
import { useSession } from '/src/hooks/useSession';
import { API_GATEWAY_URL } from '/src/config';

function LogDetailModal({ log, onClose }) {
    if (!log) return null;
    const renderValue = (value) => {
        if (value === null) return <em className="text-muted">null</em>;
        if (typeof value === 'boolean') return value ? <span className="text-success">✔️ True</span> : <span className="text-danger">❌ False</span>;
        if (typeof value === 'string' && /\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/.test(value)) {
            const d = new Date(value);
            return isNaN(d.getTime()) ? value : d.toLocaleString('vi-VN');
        }
        return value.toString();
    };
    const renderFormattedDetail = (detailStr) => {
        try {
            const parsed = JSON.parse(detailStr);
            return (
                <div className="bg-light p-3 rounded" style={{ maxHeight: '60vh', overflowY: 'auto' }}>
                    {Object.entries(parsed).map(([key, value]) => (
                        <div key={key} className="mb-3">
                            <strong className="d-block bg-dark text-white p-2 rounded-top">{key}</strong>
                            <div className="p-2 border rounded-bottom">
                                {typeof value === 'object' && value !== null ? (
                                    <table className="table table-sm table-borderless mb-0"><tbody>
                                        {Object.entries(value).map(([k, v]) => (
                                            <tr key={k}><td className="w-25"><strong>{k}</strong></td><td>{renderValue(v)}</td></tr>
                                        ))}
                                    </tbody></table>
                                ) : (renderValue(value))}
                            </div>
                        </div>
                    ))}
                </div>
            );
        } catch { return <div className="text-danger">Không thể phân tích dữ liệu chi tiết.</div>; }
    };
    return (
        <div className="modal show d-block" tabIndex="-1" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
            <div className="modal-dialog modal-dialog-centered">
                <div className="modal-content">
                    <div className="modal-header"><h5 className="modal-title">Chi tiết Log #{log.id}</h5><button type="button" className="btn-close" onClick={onClose}></button></div>
                    <div className="modal-body">{renderFormattedDetail(log.details)}</div>
                </div>
            </div>
        </div>
    );
}

function AuditLogPage() {
    const [logs, setLogs] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [currentPage, setCurrentPage] = useState(1);
    const [itemsPerPage, setItemsPerPage] = useState(10);
    const [selectedLog, setSelectedLog] = useState(null);
    const [filterAction, setFilterAction] = useState('');
    const [searchTerm, setSearchTerm] = useState('');

    const { setPageTitle, setPageIcon } = useContext(PageTitleContext);
    const { getAuthHeaders, handleSessionExpired } = useSession();

    useEffect(() => {
        setPageTitle('Nhật ký hệ thống');
        setPageIcon('fa-solid fa-clipboard-list');
    }, [setPageTitle, setPageIcon]);

    const fetchLogs = useCallback(async () => {
        setLoading(true); setError(null);
        try {
            const response = await fetch(`${API_GATEWAY_URL}/api/auditlogs`, {
                headers: getAuthHeaders()
            });
            if (response.status === 401) { handleSessionExpired(); return; }
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            const data = await response.json();
            setLogs(data);
        } catch (e) { setError(e.message); }
        finally { setLoading(false); }
    }, [getAuthHeaders, handleSessionExpired]);

    useEffect(() => { fetchLogs(); }, [fetchLogs]);

    const getActionBadgeClass = (actionType) => {
        const action = actionType.toUpperCase();
        if (action.includes('CREATE') || action.includes('RESTORE')) return 'bg-success';
        if (action.includes('UPDATE') || action.includes('EDIT')) return 'bg-warning text-dark';
        if (action.includes('DELETE')) return 'bg-danger';
        if (action.includes('LOGIN') || action.includes('SEND')) return 'bg-primary';
        return 'bg-secondary';
    };

    const formatDate = (dateString) => {
        if (!dateString) return 'N/A';
        // This will correctly parse the ISO 8601 format from the backend
        const date = new Date(dateString);
        if (isNaN(date.getTime())) return 'N/A';

        const datePart = date.toLocaleDateString('vi-VN', { weekday: 'short', year: 'numeric', month: '2-digit', day: '2-digit' });
        const timePart = date.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false });
        return `${datePart} ${timePart}`;
    };

    const filteredLogs = useMemo(() => {
        const cleanedSearchTerm = searchTerm.toLowerCase().trim();
        return logs
            .filter(log => filterAction ? log.actionType === filterAction : true)
            .filter(log => {
                if (!cleanedSearchTerm) return true;
                const usernameMatch = log.username && log.username.toLowerCase().includes(cleanedSearchTerm);
                const userIdMatch = log.userId.toString().includes(cleanedSearchTerm);
                return usernameMatch || userIdMatch;
            });
    }, [logs, filterAction, searchTerm]);

    const indexOfLastItem = currentPage * itemsPerPage;
    const indexOfFirstItem = indexOfLastItem - itemsPerPage;
    const currentLogs = filteredLogs.slice(indexOfFirstItem, indexOfLastItem);
    const totalPages = Math.ceil(filteredLogs.length / itemsPerPage);
    const actionTypes = useMemo(() => [...new Set(logs.map(log => log.actionType))], [logs]);
    const paginate = (pageNumber) => setCurrentPage(pageNumber);

    if (loading) return <div className="text-center p-5">Đang tải nhật ký...</div>;
    if (error) return <div className="alert alert-danger">Lỗi: {error}</div>;

    return (
        <div className="container-fluid pb-5">
            <div className="card mb-4">
                <div className="card-body d-flex gap-3 align-items-end">
                    <div className="flex-grow-1">
                        <label htmlFor="searchTerm" className="form-label">Tìm theo người dùng (tên hoặc ID)</label>
                        <input id="searchTerm" type="text" className="form-control" placeholder="Nhập tên hoặc ID..."
                            value={searchTerm} onChange={e => { setSearchTerm(e.target.value); setCurrentPage(1); }} />
                    </div>
                    <div>
                        <label htmlFor="filterAction" className="form-label">Lọc theo hành động</label>
                        <select id="filterAction" className="form-select" value={filterAction}
                            onChange={e => { setFilterAction(e.target.value); setCurrentPage(1); }}>
                            <option value="">Tất cả hành động</option>
                            {actionTypes.map(type => <option key={type} value={type}>{type}</option>)}
                        </select>
                    </div>
                    <div>
                        <label htmlFor="itemsPerPage" className="form-label">Số dòng</label>
                        <select id="itemsPerPage" value={itemsPerPage} onChange={e => { setItemsPerPage(Number(e.target.value)); setCurrentPage(1); }} className="form-select">
                            {[10, 20, 50, 100].map(n => <option key={n} value={n}>{n}</option>)}
                        </select>
                    </div>
                </div>
            </div>

            <div className="table-responsive">
                <table className="table table-hover table-bordered" style={{ tableLayout: 'fixed', wordWrap: 'break-word' }}>
                    <thead className="table-light">
                        <tr>
                            <th style={{ width: '15%' }}>Thời gian</th><th style={{ width: '15%' }}>Người thực hiện</th>
                            <th style={{ width: '15%' }}>Hành động</th><th style={{ width: '12%' }}>Đối tượng</th>
                            <th style={{ width: '8%' }}>ID Đối tượng</th><th style={{ width: '10%' }}>Chi tiết</th>
                        </tr>
                    </thead>
                    <tbody>
                        {currentLogs.map(log => (
                            <tr key={log.id} className="align-middle">
                                <td>{formatDate(log.timestamp)}</td>
                                <td>{log.username} (ID: {log.userId})</td>
                                <td><span className={`badge ${getActionBadgeClass(log.actionType)}`}>{log.actionType}</span></td>
                                <td>{log.entityType}</td><td>{log.entityId}</td>
                                <td><button className="btn btn-sm btn-outline-primary" onClick={() => setSelectedLog(log)}>Xem</button></td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
            {currentLogs.length === 0 && <div className="text-center p-4">Không tìm thấy kết quả phù hợp.</div>}

            <div className="d-flex justify-content-between align-items-center mt-3">
                <span className="text-muted">
                    Hiển thị từ {filteredLogs.length > 0 ? indexOfFirstItem + 1 : 0} đến {Math.min(indexOfLastItem, filteredLogs.length)} trong tổng số {filteredLogs.length} mục.
                </span>
                <nav>
                    <ul className="pagination mb-0">
                        <li className={`page-item ${currentPage === 1 ? 'disabled' : ''}`}><button onClick={() => paginate(currentPage - 1)} className="page-link">Trước</button></li>
                        {[...Array(totalPages).keys()].map(number => (
                            <li key={number + 1} className={`page-item ${currentPage === number + 1 ? 'active' : ''}`}><button onClick={() => paginate(number + 1)} className="page-link">{number + 1}</button></li>
                        ))}
                        <li className={`page-item ${currentPage === totalPages || totalPages === 0 ? 'disabled' : ''}`}><button onClick={() => paginate(currentPage + 1)} className="page-link">Sau</button></li>
                    </ul>
                </nav>
            </div>

            {selectedLog && <LogDetailModal log={selectedLog} onClose={() => setSelectedLog(null)} />}
        </div>
    );
}

export default AuditLogPage;
