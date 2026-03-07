export interface User {
  id: string;
  name: string;
  age: number;
  city: string;
  state: string;
  pincode: string;
  createdAt: string;
}

export interface CreateUserPayload {
  name: string;
  age: number;
  city: string;
  state: string;
  pincode: string;
}
