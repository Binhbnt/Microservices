import React, { useState, useEffect } from 'react';
import { useLocation, BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css';
import { useTooltip } from '/src/hooks/useTooltip';
import Tooltip from '/components/Tooltip';
import { TooltipContext } from '/src/TooltipContext';
import { NotificationProvider } from '/components/NotificationContext';

import {
  MainLayout,
  DashboardPage,
  EmployeePage,
  TrashUserList,
  UserFormModal,
  LoginPage,
  PrivateRoute,
  ReLoginModal,
  SessionWarningModal,
  ProfilePage,
  ChangePasswordPage,
  LeaveRequestPage,
  AuditLogPage,
  ActionProcessingPage,
  AllNotificationsPage,
  SystemStatusPage,
  SubscriptionManagementPage
} from '/components';

// Import các custom hooks
import { useSession } from '/src/hooks/useSession';
import { useUserData } from '/src/hooks/useUserData';

function RoleBasedRoute({ allowedRoles, children, currentUser }) {
  if (!currentUser || !allowedRoles.includes(currentUser.role)) {
    return (
      <div className="container mt-5 text-center">
        <h2 className="text-danger">Truy cập bị từ chối</h2>
        <p>Bạn không có quyền để truy cập trang này.</p>
      </div>
    );
  }
  return children;
}

function AppContent() {
  const session = useSession();
  const { setCurrentUser } = session;
  const userData = useUserData(session.currentUser, session.getAuthHeaders, session.handleSessionExpired, session.setCurrentUser);
  const location = useLocation();
  const { tooltip, showTooltip, hideTooltip } = useTooltip();

  useEffect(() => {
    if (session.toastMessage) {
      const timer = setTimeout(() => {
        session.setToastMessage(null);
      }, 3000);
      return () => clearTimeout(timer);
    }
  }, [session.toastMessage, session.setToastMessage]);

  // useEffect(() => {
  //   if (location.pathname === '/trash') {
  //     userData.fetchUsers(true);
  //   } else if (location.pathname === '/employees') {
  //     userData.fetchUsers(false);
  //   }
  // }, [location.pathname]);

  return (
    <TooltipContext.Provider value={{ showTooltip, hideTooltip }}>
      <NotificationProvider>
        <div className="app-container">
          <Routes>
            <Route
              path="/login"
              element={
                <LoginPage
                  API_GATEWAY_URL={session.API_GATEWAY_URL}
                  onLoginSuccess={session.handleLoginSuccess}
                />
              }
            />
            <Route path="/approve-leave" element={<ActionProcessingPage />} />
            <Route
              element={
                <PrivateRoute currentUser={session.currentUser}>
                  <MainLayout
                    currentUser={session.currentUser}
                    onLogout={session.handleLogout}
                    onOpenProfile={() => userData.openEditModal(session.currentUser.id)}
                  />
                </PrivateRoute>
              }
            >
              <Route path="/" element={<DashboardPage />} />
              <Route
                path="/employees"
                element={
                  <RoleBasedRoute allowedRoles={['Admin', 'SuperUser']} currentUser={session.currentUser}>
                    <EmployeePage
                      users={userData.users}
                      loading={userData.loading}
                      error={userData.error}
                      onAddUserClick={userData.openAddModal}
                      onEditUser={userData.openEditModal}
                      onDeleteUser={userData.handleDeleteUser}
                      localSearchTerm={userData.localSearchTerm}
                      setLocalSearchTerm={userData.setLocalSearchTerm}
                      localSelectedRole={userData.localSelectedRole}
                      setLocalSelectedRole={userData.setLocalSelectedRole}
                      handleFilterClick={userData.handleFilterClick}
                      onExportClick={userData.handleExportToExcel}
                      onToastMessage={session.setToastMessage}
                      currentUser={session.currentUser}
                      onRefreshUsers={userData.fetchUsers}
                      currentPage={userData.currentPage}
                      totalPages={userData.totalPages}
                      totalUsers={userData.totalUsers}
                      setCurrentPage={userData.setCurrentPage}
                    />
                  </RoleBasedRoute>
                }
              />
              <Route
                path="/profile"
                element={
                  <ProfilePage
                    currentUser={session.currentUser}
                    onOpenEditModal={() => userData.openEditModal(session.currentUser.id)}
                    onUpdateAvatar={userData.handleUpdateAvatar}
                    onToastMessage={session.setToastMessage}
                  />
                }
              />
              <Route
                path="/change-password"
                element={
                  <ChangePasswordPage
                    API_GATEWAY_URL={session.API_GATEWAY_URL}
                    getAuthHeaders={session.getAuthHeaders}
                    onLogout={session.handleLogout}
                    onToastMessage={session.setToastMessage}
                  />
                }
              />
              <Route
                path="/trash"
                element={
                  <RoleBasedRoute allowedRoles={['Admin']} currentUser={session.currentUser}>
                    <TrashUserList
                      users={userData.users}
                      loading={userData.loading}
                      error={userData.error}
                      currentUser={session.currentUser}
                      onRestoreUser={userData.handleRestoreUser}
                      onPermanentDelete={userData.handlePermanentDeleteUser}
                      onToastMessage={session.setToastMessage}
                    />
                  </RoleBasedRoute>
                }
              />
              <Route
                path="/audit-log"
                element={
                  <RoleBasedRoute allowedRoles={['Admin']} currentUser={session.currentUser}>
                    <AuditLogPage />
                  </RoleBasedRoute>
                }
              />
              <Route
                path="/leave-requests"
                element={
                  <LeaveRequestPage onToastMessage={session.setToastMessage} />
                }
              />
              <Route
                path="/subscription"
                element={
                  <RoleBasedRoute allowedRoles={['Admin', 'SuperUser']} currentUser={session.currentUser}>
                    <SubscriptionManagementPage
                      onToastMessage={session.setToastMessage}
                    />
                  </RoleBasedRoute>
                }
              />
              <Route
                path="/notifications"
                element={
                  <PrivateRoute currentUser={session.currentUser}>
                    <AllNotificationsPage />
                  </PrivateRoute>
                }
              />
              <Route
                path="/system-status"
                element={
                  <RoleBasedRoute allowedRoles={['Admin']} currentUser={session.currentUser}>
                    <SystemStatusPage />
                  </RoleBasedRoute>
                }
              />
            </Route>
          </Routes>

          <UserFormModal
            isOpen={userData.isModalOpen}
            onClose={() => userData.setIsModalOpen(false)}
            onSubmit={async (data) => {
              const result = await userData.handleSubmitUserForm(data);
              if (result) {
                session.setToastMessage(
                  result.error
                    ? { type: 'error', message: result.error }
                    : { type: 'success', message: result.message }
                );
              }
            }}
            initialData={userData.userToEdit}
          />

          <SessionWarningModal
            show={session.isRenewalModalOpen}
            onClose={session.handleDeclineRenewal}
            onRenewSession={session.handleRenewSession}
            expiresInMinutes={session.minutesRemaining}
          />

          <ReLoginModal
            show={session.isReLoginModalOpen}
            userToReLogin={session.lastActiveUser}
            onReLoginAttempt={session.handleReLoginAttempt}
            onLogout={session.handleLogout}
          />

          {session.toastMessage && (
            <div className={`custom-toast ${session.toastMessage.type}`}>
              {session.toastMessage.message}
            </div>
          )}
        </div>
      </NotificationProvider>
      <Tooltip {...tooltip} />
    </TooltipContext.Provider>
  );
}

function App() {
  return (
    <Router>
      <AppContent />
    </Router>
  );
}

export default App;