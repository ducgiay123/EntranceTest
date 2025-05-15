import React, { useState, useEffect } from "react";
import "./loginPage.css";
import LoginImage from "../asserts/login.png"; // Assuming you have this image in your assets folder
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";
import { useDispatch, useSelector } from "react-redux";
import { loginUser } from "../redux/slices/authSlice";
const LoginPage = () => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [emailError, setEmailError] = useState("");
  const [passwordError, setPasswordError] = useState("");
  const dispatch = useDispatch();
  const { loading, error, accessToken } = useSelector((state) => state.auth);

  const navigate = useNavigate();
  // Basic email validation function
  const validateEmail = (email) => {
    const emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
    return emailPattern.test(email);
  };
  const iconStyle = {
    width: "40px",
    height: "40px",
    borderRadius: "12px",
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    margin: "0 8px",
    cursor: "pointer",
  };
  // Handle form submission
  const handleLogin = async (e) => {
    e.preventDefault();

    let isValid = true;

    // Reset error messages
    setEmailError("");
    setPasswordError("");

    // Validate email
    if (!email || !validateEmail(email)) {
      setEmailError("Email is required and must be valid.");
      isValid = false;
    }

    // Validate password
    if (!password) {
      setPasswordError("Password is required.");
      isValid = false;
    }
    if (isValid) {
      const result = await dispatch(loginUser({ email, password }));
      if (result.meta.requestStatus === "fulfilled") {
        navigate("/dashboard");
      } else {
        // Show alert on login failure
        alert(
          "Login failed. Please check your email or password and try again."
        );
      }
    }
    // if (!isValid) return;

    // try {
    //   const response = await axios.post(
    //     "http://streaming.nexlesoft.com:3001/auth/signin",
    //     {
    //       email,
    //       password,
    //     }
    //   );

    //   const { accessToken, refreshToken } = response.data;

    //   // Save tokens to sessionStorage
    //   sessionStorage.setItem("accessToken", accessToken);
    //   sessionStorage.setItem("refreshToken", refreshToken);

    //   // Redirect to homepage or dashboard
    //   // window.location.href = "/navigate("/")"; // adjust as needed
    //   navigate("/dashboard"); // Adjust the path to your home route
    // } catch (error) {
    //   console.error("Login failed:", error);
    //   alert("Login failed. Please check your credentials and try again.");
    // }
  };
  useEffect(() => {
    if (accessToken) {
      navigate("/dashboard");
    }
  }, [accessToken, navigate]);
  return (
    <div className="main-container">
      <div className="left-container">
        <img className="left-img" src={LoginImage} alt="" />
      </div>

      <div className="right-container d-flex justify-content-center align-items-center">
        <div className="login-form">
          <h1 className="d-flex text-start fs-5 text-dark">
            Welcome to ReactJS Test Interview!
          </h1>
          <p className="d-flex text-start fs-15 text-muted">
            Please sign-in to your account and start the adventure
          </p>

          {/* Email Input */}
          <div className="mb-4 text-start">
            <label htmlFor="email" className="form-label">
              Email*
            </label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="johndoe@gmail.com"
              className="form-control py-3"
            />
            {emailError && <div className="text-danger mt-2">{emailError}</div>}
          </div>

          {/* Password Input */}
          <div className="mb-4">
            <div className="d-flex justify-content-between mb-2">
              <label htmlFor="password" className="form-label mb-0">
                Password*
              </label>
              <a href="#" className="theme-color small">
                Forgot Password?
              </a>
            </div>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              className="form-control py-3"
            />
            {passwordError && (
              <div className="text-danger mt-2">{passwordError}</div>
            )}
          </div>

          {/* Remember Me Checkbox */}
          <div className="d-flex align-items-center mb-3">
            <input
              type="checkbox"
              id="remember"
              className="form-check-input custom-checkbox"
            />
            <label htmlFor="remember" className="text-muted mb-0">
              &nbsp; Remember me
            </label>
          </div>

          {/* Login Button */}
          <div className="d-grid gap-2 theme-color">
            <button
              onClick={handleLogin}
              className="btn btn-theme"
              type="button"
            >
              Login
            </button>
          </div>

          {/* Link to Create Account */}
          <p className="text-center text-gray-600 mt-4">
            New on our platform?{" "}
            <Link to="/signup" className="theme-color">
              Create an account
            </Link>
          </p>

          {/* Divider with "or" */}
          <div className="d-flex align-items-center my-4">
            <hr className="flex-grow-1" />
            <span className="mx-3">or</span>
            <hr className="flex-grow-1" />
          </div>

          {/* Social Media Login Icons */}
          <div className="d-flex justify-content-center mt-4">
            {/* Facebook */}
            <div style={{ ...iconStyle, backgroundColor: "#3b5998" }}>
              <svg width="24" height="24" fill="white" viewBox="0 0 24 24">
                <path d="M22 12a10 10 0 1 0-11.5 9.87v-7h-2v-3h2v-2.3c0-2 1.2-3.1 3-3.1.9 0 1.8.1 1.8.1v2h-1c-1 0-1.3.6-1.3 1.2V12h2.3l-.4 3h-1.9v7A10 10 0 0 0 22 12z" />
              </svg>
            </div>

            {/* Twitter */}
            <div style={{ ...iconStyle, backgroundColor: "#1DA1F2" }}>
              <svg width="24" height="24" fill="white" viewBox="0 0 24 24">
                <path d="M22.46 6c-.77.35-1.6.58-2.46.69a4.3 4.3 0 0 0 1.88-2.38 8.59 8.59 0 0 1-2.72 1.04 4.28 4.28 0 0 0-7.3 3.9A12.13 12.13 0 0 1 3 4.9a4.28 4.28 0 0 0 1.32 5.7 4.23 4.23 0 0 1-1.94-.54v.06a4.28 4.28 0 0 0 3.44 4.2 4.3 4.3 0 0 1-1.93.07 4.28 4.28 0 0 0 4 3 8.6 8.6 0 0 1-5.3 1.83c-.34 0-.68-.02-1.02-.06A12.13 12.13 0 0 0 8.29 21c7.54 0 11.67-6.25 11.67-11.67 0-.18 0-.35-.01-.53A8.36 8.36 0 0 0 22.46 6z" />
              </svg>
            </div>

            {/* Email */}
            <div style={{ ...iconStyle, backgroundColor: "#d32f2f" }}>
              <svg width="24" height="24" fill="white" viewBox="0 0 24 24">
                <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.89 2 2 2h16c1.11 0 2-.9 2-2V6c0-1.1-.89-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z" />
              </svg>
            </div>

            {/* GitHub */}
            <div style={{ ...iconStyle, backgroundColor: "#24292e" }}>
              <svg width="24" height="24" fill="white" viewBox="0 0 24 24">
                <path d="M12 2C6.48 2 2 6.58 2 12.26c0 4.5 2.87 8.32 6.84 9.66.5.1.66-.22.66-.48v-1.7c-2.78.61-3.37-1.34-3.37-1.34-.45-1.17-1.1-1.48-1.1-1.48-.9-.63.07-.62.07-.62 1.01.08 1.54 1.04 1.54 1.04.9 1.53 2.36 1.1 2.93.84.09-.65.35-1.08.63-1.34-2.22-.26-4.56-1.12-4.56-5 0-1.1.39-1.99 1.03-2.7-.1-.25-.45-1.28.1-2.66 0 0 .84-.27 2.75 1.02a9.58 9.58 0 0 1 5 0c1.91-1.29 2.75-1.02 2.75-1.02.55 1.38.2 2.4.1 2.66.64.71 1.03 1.6 1.03 2.7 0 3.88-2.34 4.74-4.56 5 .36.31.68.92.68 1.85v2.75c0 .26.16.58.66.48A10.01 10.01 0 0 0 22 12.26C22 6.58 17.52 2 12 2z" />
              </svg>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;
