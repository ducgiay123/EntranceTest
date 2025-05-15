import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import SignupImage from "../asserts/signUp.png"; // Assuming you have this image in your assets folder
import axios from "axios";

const SignupPage = () => {
  const [formData, setFormData] = useState({
    firstname: "",
    lastname: "",
    email: "",
    password: "",
    agree: false,
  });

  const [errors, setErrors] = useState({});
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
  const navigate = useNavigate();
  const validate = () => {
    const errs = {};
    if (!formData.firstname.trim()) errs.firstname = "Firstname is required";
    if (!formData.lastname.trim()) errs.lastname = "Lastname is required";
    if (!formData.email.match(/^[^\s@]+@[^\s@]+\.[^\s@]+$/))
      errs.email = "Invalid email address";
    if (formData.password.length < 6)
      errs.password = "Password must be at least 6 characters";
    if (!formData.agree) errs.agree = "You must agree to the terms";
    return errs;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const errs = validate();
    setErrors(errs);
    console.log(formData);
    if (Object.keys(errs).length === 0) {
      try {
        // Make API request to create account
        const response = await axios.post(
          "http://streaming.nexlesoft.com:3001/auth/signup",
          {
            firstName: formData.firstname,
            lastName: formData.lastname,
            email: formData.email,
            password: formData.password,
          }
        );
        console.log("Account created successfully:", response.data);

        // Show success alert
        alert("Account created successfully! Please sign in.");

        // Redirect to the sign-in page
        navigate("/"); // Adjust the path to your sign-in route
      } catch (error) {
        console.error("Error creating account:", error);
        // Handle error (e.g., show an error message)
        alert("Error creating account. Please try again.");
      }
    }
  };

  return (
    <div className="main-container">
      <div className="left-container">
        <img className="left-img" src={SignupImage} alt="" />
      </div>

      <div className="right-container d-flex justify-content-center align-items-center">
        <div className="login-form">
          <h1 className="d-flex text-start fs-5 text-dark">
            Adventure starts here
          </h1>
          <p className="d-flex text-start fs-15 text-muted">
            Make your app management easy and fun!
          </p>

          <form onSubmit={handleSubmit} noValidate>
            <div className="mb-3">
              <label className="d-flex form-label text-start">Firstname*</label>
              <input
                type="text"
                className={`form-control ${errors.firstname && "is-invalid"}`}
                placeholder="johndoe"
                value={formData.firstname}
                onChange={(e) =>
                  setFormData({ ...formData, firstname: e.target.value })
                }
              />
              <div className="invalid-feedback">{errors.firstname}</div>
            </div>

            <div className="mb-3">
              <label className="d-flex form-label text-start">Lastname*</label>
              <input
                type="text"
                className={`form-control ${errors.lastname && "is-invalid"}`}
                placeholder="johndoe"
                value={formData.lastname}
                onChange={(e) =>
                  setFormData({ ...formData, lastname: e.target.value })
                }
              />
              <div className="invalid-feedback">{errors.lastname}</div>
            </div>

            <div className="mb-3">
              <label className="d-flex form-label text-start">Email *</label>
              <input
                type="email"
                className={`form-control ${errors.email && "is-invalid"}`}
                placeholder="johndoe@gmail.com"
                value={formData.email}
                onChange={(e) =>
                  setFormData({ ...formData, email: e.target.value })
                }
              />
              <div className="invalid-feedback">{errors.email}</div>
            </div>

            <div className="mb-3">
              <label className="d-flex form-label text-start">Password*</label>
              <input
                type="password"
                className={`form-control ${errors.password && "is-invalid"}`}
                placeholder="••••••••"
                value={formData.password}
                onChange={(e) =>
                  setFormData({ ...formData, password: e.target.value })
                }
              />
              <div className="invalid-feedback">{errors.password}</div>
            </div>

            <div className="form-check mb-3">
              <input
                type="checkbox"
                className={`form-check-input custom-checkbox ${
                  errors.agree && "is-invalid"
                }`}
                id="agree"
                checked={formData.agree}
                onChange={(e) =>
                  setFormData({ ...formData, agree: e.target.checked })
                }
              />
              <label
                className="form-check-label d-flex flex-start"
                htmlFor="agree"
              >
                I agree to &nbsp;
                <a href="#" className="theme-color ">
                  privacy policy & terms
                </a>
              </label>
              <div className="invalid-feedback">{errors.agree}</div>
            </div>

            <button type="submit" className="btn btn-theme w-100">
              Sign Up
            </button>
          </form>
          <p className="text-center mt-3 text-muted">
            Already have an account?{" "}
            <Link to="/" className="theme-color">
              Sign In Instead
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

export default SignupPage;
