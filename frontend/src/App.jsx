import { BrowserRouter, Navigate, Route, Routes} from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ConfirmPage from "./pages/ConfirmPage";
import UsersPage from "./pages/UsersPage";

function App() {
  return (
    <BrowserRouter>
        <Routes>
            <Route path="/" element={<Navigate to="/login" replace />} />
            <Route path="/login" element={<LoginPage/>} />
            <Route path="/register" element={<RegisterPage/>} />
            <Route path="/confirm" element={<ConfirmPage />} />
            <Route path="/users" element={<UsersPage />} />
        </Routes>
    </BrowserRouter>
  )
}

export default App;